/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        MessageEndPoint.cs 
 * Types:           AI4E.Modularity.MessageEndPoint
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   06.07.2017 
 * --------------------------------------------------------------------------------------------------------------------
 */

/* License
 * --------------------------------------------------------------------------------------------------------------------
 * This file is part of the AI4E distribution.
 *   (https://gitlab.com/EnterpriseApplicationEquipment/AI4E)
 * Copyright (c) 2017 Andreas Trütschel.
 * 
 * AI4E is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU Lesser General Public License as   
 * published by the Free Software Foundation, version 3.
 *
 * AI4E is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 * --------------------------------------------------------------------------------------------------------------------
 */

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AI4E.Async;
using AI4E.Async.Processing;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using static System.Diagnostics.Debug;

namespace AI4E.Modularity
{
    /// <summary>
    /// A message endpoint that can send messages to remote endpoints, receives messages and delivers them to message handlers.
    /// </summary>
    public sealed partial class MessageEndPoint : IMessageEndPoint
    {
        #region Constants

        private const uint _currentVersion = 1; // Current protocol version.
        private const int _headerLength = 20; // Header length in current protocol version. (Can only increase in future versions)
        private static readonly byte[] _emptyPayload = new byte[0];

        #endregion

        #region Fields

        private readonly Stream _underlyingStream;
        private readonly IMessageSerializer _serializer;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly AsyncProcess _receiveProcess;
        private readonly TaskCompletionSource<object> _completion = new TaskCompletionSource<object>();
        private readonly Task _initialization;
        private readonly AsyncLock _sendLock = new AsyncLock();
        private readonly ConcurrentDictionary<uint, TaskCompletionSource<object>> _responseTable = new ConcurrentDictionary<uint, TaskCompletionSource<object>>();
        private readonly ConcurrentDictionary<Type, IMessageReceiver> _messageReceivers = new ConcurrentDictionary<Type, IMessageReceiver>();

        // TODO: Send-Queue, Receive-Queue non-volatile storage backed.

        private Task _completing;
        private int _nextSeqNum = 1;

        #endregion

        #region C'tors

        /// <summary>
        /// Creates a new instance of the <see cref="MessageEndPoint"/> type with the specified underlying stream.
        /// </summary>
        /// <param name="underlyingStream">The stream that is used to send and receive raw message data.</param>
        /// <param name="serializer">A binary json (bson) serializer.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> used to obtain services.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any of <paramref name="underlyingStream"/>, <paramref name="serializer"/> or <paramref name="serviceProvider"/> is null.
        /// </exception>
        public MessageEndPoint(Stream underlyingStream, IMessageSerializer serializer, IServiceProvider serviceProvider)
        {
            if (underlyingStream == null)
                throw new ArgumentNullException(nameof(underlyingStream));

            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            _underlyingStream = underlyingStream;
            _serializer = serializer;
            _serviceProvider = serviceProvider;
            _receiveProcess = new AsyncProcess(ReceiveProcedure);
            _initialization = InitializeAsync();
        }

        public MessageEndPoint(Stream underlyingStream,
                               IMessageSerializer serializer,
                               ILogger<MessageEndPoint> logger,
                               IServiceProvider serviceProvider) : this(underlyingStream, serializer, serviceProvider)
        {
            _logger = logger;
        }

        #endregion

        #region Handler

        /// <summary>
        /// Asynchronously registers a message handler.
        /// </summary>
        /// <typeparam name="TMessage">The type of message the handler handles.</typeparam>
        /// <param name="handlerFactory">The message handler to register.</param>
        /// <returns>
        /// A <see cref="IHandlerRegistration"/> representing the asynchronous operation.
        /// The <see cref="IHandlerRegistration"/> cancels the handler registration if completed.
        /// </returns>
        ///  <exception cref="ArgumentNullException">Thrown if <paramref name="handlerFactory"/> is null.</exception>
        public async Task<IHandlerRegistration> RegisterAsync<TMessage>(IContextualProvider<IMessageHandler<TMessage>> handlerFactory) // TODO: Correct xml-comments
        {
            if (handlerFactory == null)
                throw new ArgumentNullException(nameof(handlerFactory));

            ThrowIfDisposed();
            await Initialization;

            _logger?.LogInformation($"Registering handler for message type '{typeof(TMessage).FullName}'.");

            var result = await GetMessageReceiver<TMessage>().RegisterAsync(handlerFactory);

            _logger?.LogInformation($"Registered handler for message type '{typeof(TMessage).FullName}'.");

            return result;
        }

        /// <summary>
        /// Asynchronously registers a message handler.
        /// </summary>
        /// <typeparam name="TMessage">The type of message the handler handles.</typeparam>
        /// <typeparam name="TResponse">The type of response the handler returns.</typeparam>
        /// <param name="handlerFactory">The message handler to register.</param>
        /// <returns>
        /// A <see cref="IHandlerRegistration"/> representing the asynchronous operation.
        /// The <see cref="IHandlerRegistration"/> cancels the handler registration if completed.
        /// </returns>
        ///  <exception cref="ArgumentNullException">Thrown if <paramref name="handlerFactory"/> is null.</exception>
        public async Task<IHandlerRegistration> RegisterAsync<TMessage, TResponse>(IContextualProvider<IMessageHandler<TMessage, TResponse>> handlerFactory) // TODO: Correct xml-comments
        {
            if (handlerFactory == null)
                throw new ArgumentNullException(nameof(handlerFactory));

            ThrowIfDisposed();
            await Initialization;

            _logger?.LogInformation($"Registering handler for message type '{typeof(TMessage).FullName}' and result type '{typeof(TMessage).FullName}'.");

            var result = await GetMessageReceiver<TMessage>().RegisterAsync(handlerFactory);

            _logger?.LogInformation($"Registered handler for message type '{typeof(TMessage).FullName}' and result type '{typeof(TMessage).FullName}'.");

            return result;
        }

        private MessageReceiver<TMessage> GetMessageReceiver<TMessage>()
        {
            return (MessageReceiver<TMessage>)_messageReceivers.GetOrAdd(typeof(TMessage), type => new MessageReceiver<TMessage>(this));
        }

        #endregion

        #region Send

        /// <summary>
        /// Asynchronously sends the specified message to the remote endpoint and awaits its answer.
        /// </summary>
        /// <typeparam name="TMessage">The type of message.</typeparam>
        /// <param name="message">The message to send to the remove end point.</param>
        /// <param name="cancellation">A <see cref="CancellationToken"/> used to cancel the asynchronous operation or <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
        public Task SendAsync<TMessage>(TMessage message, CancellationToken cancellation = default)
        {
            return SendInternalAsync(message, cancellation);
        }

        /// <summary>
        /// Asynchronously sends the specified message to the remote endpoint and awaits its answer.
        /// </summary>
        /// <typeparam name="TMessage">The type of message.</typeparam>
        /// <typeparam name="TResponse">The type of response.</typeparam>
        /// <param name="message">The message to send to the remove end point.</param>
        /// <param name="cancellation">A <see cref="CancellationToken"/> used to cancel the asynchronous operation or <see cref="CancellationToken.None"/>.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The <see cref="Task{TResult}.Result"/> contains the response of the message end point 
        /// or the default value of <typeparamref name="TResponse"/> if either the message end point did not send a response or it was of incompatible type.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
        public async Task<TResponse> SendAsync<TMessage, TResponse>(TMessage message, CancellationToken cancellation = default)
        {
            if ((await SendInternalAsync(message, cancellation)) is TResponse response)
            {
                return response;
            }

            return default;
        }

        private static readonly MessageEncoding usedEncoding = MessageEncoding.Json; // TODO: This should be determined when the connection is established.

        private async Task<object> SendInternalAsync<TMessage>(TMessage message, CancellationToken cancellation)
        {
            ThrowIfDisposed();
            await Initialization;

            // Reserve a new slot in the response table.
            using (var responseTableSlot = new ResponseTableSlot(this))
            {
                _logger?.LogInformation($"Sending message of type '{typeof(TMessage).FullName}' with seq-num '{responseTableSlot.SeqNum}'.");

                cancellation.ThrowIfCancellationRequested();

                try
                {
                    // Serialize the message and send it.
                    await SendPayloadAsync(_serializer.Serialize(message, usedEncoding), responseTableSlot.SeqNum, 0, MessageType.Message, usedEncoding, cancellation);
                }
                catch (Exception exc) when (!(exc is ObjectDisposedException))
                {
                    _logger?.LogError($"The message with the seq-num '{responseTableSlot.SeqNum}' could not be sent.", exc);

                    throw;
                }

                _logger?.LogInformation($"Sent message with seq-num '{responseTableSlot.SeqNum}'. Waiting for response.");

                try
                {
                    // Wait for a reponse from the remote end-point or cancellation alternatively.
                    var completed = await Task.WhenAny(responseTableSlot.Response, cancellation.AsTask());

                    if (completed != responseTableSlot.Response)
                    {
                        throw new TaskCanceledException();
                    }

                    var result = await responseTableSlot.Response;
                    _logger?.LogInformation($"Received response for the message with seq-num '{responseTableSlot.SeqNum}'. (Result)");
                    return result;
                }
                catch (MessageHandlerNotFoundException)
                {
                    _logger?.LogInformation($"The receiver cannot handle the message with seq-num '{responseTableSlot.SeqNum}'. No suitable message handler was found.");

                    throw;
                }
                catch (Exception exc) when (!(exc is OperationCanceledException))
                {
                    _logger?.LogInformation($"Received response for the message with seq-num '{responseTableSlot.SeqNum}'. (Exception)");

                    throw;
                }
            }
        }

        private async Task SendPayloadAsync(byte[] payload, uint seqNum, uint corrId, MessageType type, MessageEncoding encoding, CancellationToken cancellation)
        {
            Assert(payload != null);
            Assert(type > 0 && type <= MessageType.MaxEnum);
            Assert(encoding > 0 && (byte)encoding <= 0xF || encoding == MessageEncoding.Unkown && payload.Length == 0);
            Assert(payload.GetLongLength(0) <= int.MaxValue);

            var packetLength = payload.Length + _headerLength;
            var padding = (4 * ((packetLength + 3) / 4) - packetLength);

            Assert(padding >= 0 && padding <= 3);
            Assert((packetLength + padding) % 4 == 0);

            // Build the packet header.
            var header = new byte[_headerLength];

            using (var memStream = new MemoryStream(header))
            using (var binaryWriter = new BinaryWriter(memStream)) // TODO: Byte-order?
            {
                binaryWriter.Write(packetLength);   // 4 byte // Packet length without padding
                binaryWriter.Write((uint)1);        // 4 byte // Packet protocol version
                binaryWriter.Write(seqNum);         // 4 byte // Sequence number (seq-num)
                binaryWriter.Write((ushort)type);   // |      // Packet type
                binaryWriter.Write((byte)encoding); // 4 byte // Packet encoding
                binaryWriter.Write((byte)0);        // |      // (Reserved for future use)      
                binaryWriter.Write(corrId);         // 4 byte // Correlation id
            }

            try
            {
                using (await _sendLock.LockAsync())
                {
                    // Send the packet header.
                    await _underlyingStream.WriteAsync(header, 0, header.Length, cancellation);

                    // Only send a payload and padding if payload is available.
                    if (payload.Length > 0)
                    {
                        // Send the payload.
                        await _underlyingStream.WriteAsync(payload, 0, payload.Length, cancellation);

                        // Just send anything to pad the message.
                        await _underlyingStream.WriteAsync(header, 0, padding, cancellation);
                    }
                    else
                    {
                        // Payload is empty and the header length is a multiple of 4 => There must be no padding
                        Assert(padding == 0);
                    }

                    await _underlyingStream.FlushAsync();
                }
            }
            catch (IOException exc) when (IsDisposed)
            {
                throw ObjectDisposedException(exc);
            }
        }

        private Task SendProtocolVersionNotSupportedErrorAsync(uint corrId, CancellationToken cancellation)
        {
            return SendPayloadAsync(_emptyPayload, GetNextSeqNum(), corrId, MessageType.ProtocolNotSupportedError, MessageEncoding.Unkown, cancellation);
        }

        private Task SendInitAsync(uint seqNum, CancellationToken cancellation)
        {
            return SendPayloadAsync(_emptyPayload, seqNum, 0, MessageType.Init, MessageEncoding.Unkown, cancellation);
        }

        private Task SendInitAckAsync(uint corrId, CancellationToken cancellation)
        {
            return SendPayloadAsync(_emptyPayload, GetNextSeqNum(), corrId, MessageType.InitAck, MessageEncoding.Unkown, cancellation);
        }

        private Task SendTerminateAsync(uint seqNum, CancellationToken cancellation)
        {
            return SendPayloadAsync(_emptyPayload, seqNum, 0, MessageType.Terminate, MessageEncoding.Unkown, cancellation);
        }

        private Task SendTerminateAckAsync(uint corrId, CancellationToken cancellation)
        {
            return SendPayloadAsync(_emptyPayload, GetNextSeqNum(), corrId, MessageType.TerminateAck, MessageEncoding.Unkown, cancellation);
        }

        private Task SendMessageReceivedAsync(uint corrId, CancellationToken cancellation)
        {
            return SendPayloadAsync(_emptyPayload, GetNextSeqNum(), corrId, MessageType.MessageReceived, MessageEncoding.Unkown, cancellation);
        }

        private Task SendBadMessageAsync(uint corrId, CancellationToken cancellation)
        {
            return SendPayloadAsync(_emptyPayload, GetNextSeqNum(), corrId, MessageType.BadMessage, MessageEncoding.Unkown, cancellation);
        }

        private Task SendUnknownErrorAsync(uint corrId, CancellationToken cancellation)
        {
            return SendPayloadAsync(_emptyPayload, GetNextSeqNum(), corrId, MessageType.UnknownError, MessageEncoding.Unkown, cancellation);
        }

        private async Task RequestInitAsync(CancellationToken cancellation)
        {
            using (var resonseTableSlot = new ResponseTableSlot(this))
            {
                // Send init message
                await SendInitAsync(resonseTableSlot.SeqNum, cancellation);

                // Wait for init ack
                await Task.WhenAny(resonseTableSlot.Response, cancellation.AsTask());
            }
        }

        private async Task RequestTerminationAsync(CancellationToken cancellation)
        {
            using (var resonseTableSlot = new ResponseTableSlot(this))
            {
                // Send termination message
                await SendTerminateAsync(resonseTableSlot.SeqNum, cancellation);

                // Wait for terminate ack
                await Task.WhenAny(resonseTableSlot.Response, cancellation.AsTask());
            }
        }

        /// <summary>
        /// Gets the next available legal seq-num.
        /// </summary>
        private uint GetNextSeqNum()
        {
            uint result;

            do
            {
                result = unchecked((uint)Interlocked.Increment(ref _nextSeqNum));
            }
            while (result == 0);

            return result;
        }

        /// <summary>
        /// Represents a slot in the reponse table.
        /// </summary>
        private struct ResponseTableSlot : IDisposable
        {
            private readonly MessageEndPoint _messageEndPoint;
            private readonly uint _seqNum;
            private readonly TaskCompletionSource<object> _completionSource;

            /// <summary>
            /// Creates a new slot in the response table.
            /// </summary>
            /// <param name="messageEndPoint">The message end point.</param>
            public ResponseTableSlot(MessageEndPoint messageEndPoint)
            {
                Assert(messageEndPoint != null);

                _messageEndPoint = messageEndPoint;
                _seqNum = 0;

                _completionSource = new TaskCompletionSource<object>();

                // Allocate the next free seq-num that must not habe an entry in the response table.
                do
                {
                    _seqNum = _messageEndPoint.GetNextSeqNum();
                }
                while (!_messageEndPoint._responseTable.TryAdd(_seqNum, _completionSource));
            }

            /// <summary>
            /// Gets the seq-num of the response table slot.
            /// </summary>
            public uint SeqNum => _seqNum;

            /// <summary>
            /// Gets the task that represens the response.
            /// </summary>
            public Task<object> Response => _completionSource.Task;

            /// <summary>
            /// Removes the entry from the reponse table.
            /// </summary>
            public void Dispose()
            {
                _messageEndPoint._responseTable.TryRemove(_seqNum, out _);
            }
        }

        #endregion

        #region Receive

        private async Task ReceiveProcedure(CancellationToken cancellation)
        {
            _logger?.LogInformation($"Started receive procedure.");

            while (cancellation.ThrowOrContinue())
            {
                try
                {
                    var (payload, type, encoding, seqNum, corrId) = await ReceiveMessageAsync(cancellation);

                    _logger?.LogInformation($"Received message of type {type} with seq-num {seqNum}.");

                    if (type != MessageType.Unknown)
                    {
                        HandleMessageAsync(payload, type, encoding, seqNum, corrId, cancellation).HandleExceptions();
                    }
                }
                catch (ObjectDisposedException) when (IsDisposed)
                {
                    break;
                }
            }
        }

        private async Task<(byte[] payload, MessageType type, MessageEncoding encoding, uint seqNum, uint corrId)> ReceiveMessageAsync(CancellationToken cancellation)
        {
            var header = new byte[_headerLength];
            int packetLength; // Does NOT include padding.
            uint version;
            uint seqNum;
            uint corrId;
            int headerLength;
            int padding;
            MessageType type;
            MessageEncoding encoding;
            byte[] payload;

            try
            {
                await _underlyingStream.ReadExactAsync(header, 0, 12, cancellation);

                using (var memStream = new MemoryStream(header))
                using (var binaryReader = new BinaryReader(memStream)) // TODO: Byte-order?
                {
                    packetLength = binaryReader.ReadInt32();
                    version = binaryReader.ReadUInt32();
                    seqNum = binaryReader.ReadUInt32();

                    if (version == 1)
                    {
                        headerLength = 20;

                        await _underlyingStream.ReadExactAsync(header, 12, headerLength - 12, cancellation);

                        type = (MessageType)binaryReader.ReadUInt16();
                        encoding = (MessageEncoding)binaryReader.ReadByte();
                        binaryReader.ReadByte();
                        corrId = binaryReader.ReadUInt32();
                        padding = (4 * ((packetLength + 3) / 4) - packetLength);

                        Assert(padding >= 0 && padding <= 3);
                    }
                    else
                    {
                        await SendProtocolVersionNotSupportedErrorAsync(seqNum, cancellation);
                        return (_emptyPayload, MessageType.Unknown, MessageEncoding.Unkown, 0, 0);
                    }
                }

                var payloadLength = packetLength - headerLength;
                payload = new byte[payloadLength];

                if (payloadLength > 0)
                {
                    await _underlyingStream.ReadExactAsync(payload, 0, payload.Length, cancellation);
                    await _underlyingStream.ReadExactAsync(header, 0, padding, cancellation);
                }
                else
                {
                    // Payload is empty && the header length is a multiple of 4 => There is no padding
                    Assert(padding == 0);
                }
            }
            catch (IOException exc) when (IsDisposed)
            {
                throw ObjectDisposedException(exc);
            }

            return (payload, type, encoding, seqNum, corrId);
        }

        private async Task HandleMessageAsync(byte[] payload, MessageType type, MessageEncoding encoding, uint seqNum, uint corrId, CancellationToken cancellation)
        {
            try
            {
                switch (type)
                {
                    case MessageType.Init:
                        if (encoding != MessageEncoding.Unkown)
                        {
                            goto SEND_BAD_MESSAGE;
                        }

                        await SendInitAckAsync(seqNum, cancellation);
                        return;

                    case MessageType.InitAck:
                        if (encoding != MessageEncoding.Unkown)
                        {
                            goto SEND_BAD_MESSAGE;
                        }

                        HandleInitAck(payload, corrId);
                        return;

                    case MessageType.Terminate:
                        if (encoding != MessageEncoding.Unkown)
                        {
                            goto SEND_BAD_MESSAGE;
                        }

                        Complete(); // Terminate the end point

                        await SendTerminateAckAsync(seqNum, cancellation);
                        return;

                    case MessageType.TerminateAck:
                        if (encoding != MessageEncoding.Unkown)
                        {
                            goto SEND_BAD_MESSAGE;
                        }

                        HandleTerminateAck(payload, corrId);
                        return;

                    case MessageType.Message:
                        if (encoding < MessageEncoding.BinarySerialized || encoding > MessageEncoding.Bson)
                        {
                            goto SEND_BAD_MESSAGE;
                        }

                        if ((_serializer.SupportedEncodings & encoding) != encoding)
                        {
                            goto SEND_BAD_MESSAGE; // TODO: Send unsupported encoding message.
                        }

                        await HandleUserMessageAsync(payload, encoding, seqNum, cancellation);
                        return;

                    case MessageType.MessageReceived:
                        return;

                    case MessageType.MessageHandled:
                        if (encoding < MessageEncoding.BinarySerialized || encoding > MessageEncoding.Bson)
                        {
                            goto SEND_BAD_MESSAGE;
                        }

                        HandleMessageHandled(payload, corrId, encoding);
                        return;

                    case MessageType.MessageError:
                        if (encoding < MessageEncoding.BinarySerialized || encoding > MessageEncoding.Bson)
                        {
                            goto SEND_BAD_MESSAGE;
                        }

                        HandleMessageError(payload, corrId, encoding);
                        return;

                    case MessageType.ProtocolNotSupportedError:
                        // TODO
                        return;

                    case MessageType.BadMessage:
                    case MessageType.UnknownError:
                        // TODO: Log failure
                        return;
                }


                SEND_BAD_MESSAGE:
                await SendBadMessageAsync(seqNum, cancellation);
            }
            catch
            {
                await SendUnknownErrorAsync(seqNum, cancellation);

                throw;
            }
        }

        private void HandleInitAck(byte[] payload, uint corrId)
        {
            // TODO: How do we know that the tcs is used from the init proc?
            if (_responseTable.TryGetValue(corrId, out var completionSource))
            {
                completionSource.TrySetResult(null);
            }
        }

        private void HandleTerminateAck(byte[] payload, uint corrId)
        {
            // TODO: How do we know that the tcs is used from the terminate proc?
            if (_responseTable.TryGetValue(corrId, out var completionSource))
            {
                completionSource.TrySetResult(null);
            }
        }

        private async Task HandleUserMessageAsync(byte[] payload, MessageEncoding encoding, uint seqNum, CancellationToken cancellation)
        {
            object data = null;

            if (encoding != MessageEncoding.Unkown && encoding != MessageEncoding.Raw)
            {
                // TODO: It may be the case that we cannot deserialize the response because the remote end defines it.
                data = _serializer.Deserialize(payload, encoding);
            }

            if (data == null)
            {
                await SendBadMessageAsync(seqNum, cancellation);
            }

            try
            {
                await SendMessageReceivedAsync(seqNum, cancellation);

                if (!TryGetMessageReceiver(data.GetType(), out var messageReceiver))
                {
                    throw new MessageHandlerNotFoundException();
                }

                await messageReceiver.HandleMessage(data, seqNum);
            }
            catch (Exception exc)
            {
                var responsePayload = _serializer.Serialize(exc, usedEncoding);

                await SendPayloadAsync(responsePayload, GetNextSeqNum(), seqNum, MessageType.MessageError, usedEncoding, cancellation);
            }
        }

        private void HandleMessageError(byte[] payload, uint corrId, MessageEncoding encoding)
        {
            // TODO: It may be the case that we cannot deserialize the error because the remote end defines it.
            var error = _serializer.Deserialize(payload, encoding) as Exception;
            if (_responseTable.TryGetValue(corrId, out var completionSource))
            {
                completionSource.TrySetException(error);
            }
        }

        private void HandleMessageHandled(byte[] payload, uint corrId, MessageEncoding encoding)
        {
            object response = null;

            if (encoding != MessageEncoding.Unkown)
            {
                // TODO: It may be the case that we cannot deserialize the response because the remote end defines it.
                response = _serializer.Deserialize(payload, encoding);
            }

            if (_responseTable.TryGetValue(corrId, out var completionSource))
            {
                completionSource.TrySetResult(response);
            }
        }

        private bool TryGetMessageReceiver(Type messageType, out IMessageReceiver messageReceiver)
        {
            return _messageReceivers.TryGetValue(messageType, out messageReceiver);
        }

        #endregion

        #region Initialization, Completion

        /// <summary>
        /// Gets a task representing the asynchronous initialization of the current instance.
        /// </summary>
        public Task Initialization => _initialization;

        /// <summary>
        /// Gets a task that represents the asynchronous completion of the instance.
        /// </summary>
        public Task Completion => _completion.Task;

        /// <summary>
        /// Starts completing the instance asynchronously.
        /// </summary>
        /// <remarks>
        /// This is conceptually similar to <see cref="IDisposable.Dispose"/>.
        /// After calling this method, invoking any member except <see cref="Completion"/> is forbidden.
        /// </remarks>
        public void Complete()
        {
            if (_completing != null)
                return;

            _completing = CompleteAsync();
        }

        private async Task InitializeAsync()
        {
            // Start receive proc
            _receiveProcess.StartExecution();

            _receiveProcess.Execution.HandleExceptions();

            // Send init message & await init ack
            await RequestInitAsync(default);
        }

        private async Task CompleteAsync()
        {
            await Initialization;

            try
            {
                // Send terminate message & await terminate ack
                await RequestTerminationAsync(default);

                // Terminate receive proc
                _receiveProcess.TerminateExecution();
            }
            catch (Exception exc)
            {
                _completion.TrySetException(exc);
            }
            finally
            {
                _underlyingStream.Close();
            }

            _completion.TrySetResult(null);
        }

        private bool IsDisposed => _completing != null;

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                _logger?.LogError("The end point is disposed and cannot be used.");

                throw ObjectDisposedException();
            }
        }

        private T ThrowIfDisposed<T>(T t)
        {
            ThrowIfDisposed();

            return t;
        }

        private ObjectDisposedException ObjectDisposedException()
        {
            return new ObjectDisposedException(GetType().FullName);
        }

        private ObjectDisposedException ObjectDisposedException(Exception innerException)
        {
            return new ObjectDisposedException(GetType().FullName, innerException);
        }

        #endregion
    }
}
