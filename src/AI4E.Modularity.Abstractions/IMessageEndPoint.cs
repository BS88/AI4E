/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        IMessageEndPoint.cs 
 * Types:           (1) AI4E.Modularity.IMessageEndPoint
 *                  (2) AI4E.Modularity.MessageHandlerNotFoundException
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   16.06.2017 
 * Status:          Ready
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
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AI4E.Async;

namespace AI4E.Modularity
{
    /// <summary>
    /// Represents a message endpoint that can send messages to remove endpoints 
    /// and receive messages that are delivered to message handlers.
    /// </summary>
    public interface IMessageEndPoint : IAsyncInitialization, IAsyncCompletion
    {
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
        Task<IHandlerRegistration> RegisterAsync<TMessage>(IContextualProvider<IMessageHandler<TMessage>> handlerFactory); // TODO: Correct xml-comments

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
        Task<IHandlerRegistration> RegisterAsync<TMessage, TResponse>(IContextualProvider<IMessageHandler<TMessage, TResponse>> handlerFactory); // TODO: Correct xml-comments

        /// <summary>
        /// Asynchronously send the specified message to the remote endpoint and awaits its answer.
        /// </summary>
        /// <typeparam name="TMessage">The type of message.</typeparam>
        /// <param name="message">The message to send to the remove end point.</param>
        /// <param name="cancellation">A <see cref="CancellationToken"/> used to cancel the asynchronous operation or <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
        Task SendAsync<TMessage>(TMessage message, CancellationToken cancellation = default(CancellationToken));

        /// <summary>
        /// Asynchronously send the specified message to the remote endpoint and awaits its answer.
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
        Task<TResponse> SendAsync<TMessage, TResponse>(TMessage message, CancellationToken cancellation = default(CancellationToken));
    }

    [Serializable]
    public sealed class MessageHandlerNotFoundException : Exception
    {
        public MessageHandlerNotFoundException() : base("No message handler for the message could be found.") { }

        public MessageHandlerNotFoundException(string message) : base(message) { }

        public MessageHandlerNotFoundException(string message, Exception innerException) : base(message, innerException) { }

        private MessageHandlerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
