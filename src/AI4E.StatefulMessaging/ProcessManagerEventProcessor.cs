using AI4E.Storage;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AI4E.Integration
{
    public sealed class ProcessManagerEventProcessor : EventProcessor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataStore _dataStore;

        private bool _created;
        private object _state;

        public ProcessManagerEventProcessor(IServiceProvider serviceProvider, IDataStore dataStore)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (dataStore == null)
                throw new ArgumentNullException(nameof(dataStore));

            _serviceProvider = serviceProvider;
            _dataStore = dataStore;
        }

        public override async Task<TEvent> PreProcessAsync<TEvent>(TEvent evt)
        {
            if (!IsProcessManager)
            {
                return evt;
            }

            var processManagerStateProperty = EventHandlerType.GetProperties().SingleOrDefault(p => p.CanWrite && p.IsDefined<ProcessManagerStateAttribute>());

            if (processManagerStateProperty != null)
            {
                var stateType = processManagerStateProperty.PropertyType;
                {
                    var customType = processManagerStateProperty.GetCustomAttribute<ProcessManagerStateAttribute>().StateType;

                    if (customType != null)
                    {
                        if (!stateType.IsAssignableFrom(customType))
                        {
                            throw new InvalidOperationException(); // TODO
                        }
                        stateType = customType;
                    }
                }

                var attachments = Activator.CreateInstance(typeof(ProcessAttachments<>).MakeGenericType(stateType), _dataStore);

                Debug.Assert(attachments != null);

                var attachMethod = EventHandlerType.GetMethod("AttachProcess", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (attachMethod == null)
                {
                    throw new InvalidOperationException(); // TODO
                }

                attachMethod.Invoke(EventHandler, new[] { attachments });

                //EventHandler.AttachProcessManager(attachments);

                _state = (object)(await ((dynamic)attachments).GetStateAsync(evt));

                // TODO: Not all events are allowed to start a process.
                if (_state == null)
                {
                    var canInitMethodDef = EventHandlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(p => p.Name == "CanInitiateProcess" && p.IsGenericMethodDefinition && p.GetGenericArguments().Length == 1);
                    var canInitMethod = canInitMethodDef?.MakeGenericMethod(typeof(TEvent));


                    if (canInitMethod != null && !(bool)canInitMethod.Invoke(EventHandler, new object[] { evt })) //!(bool)(EventHandler.CanInitiateProcess(evt)))
                    {
                        return evt;

                        // return new FailureEventResult(""); // TODO: Maybe the events are out of order? What to do about that?
                    }

                    // Create state

                    var createInitStateMethodDef = EventHandlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(p => p.Name == "CreateInitialState" && p.IsGenericMethodDefinition && p.GetGenericArguments().Length == 1);
                    var createInitStateMethod = createInitStateMethodDef?.MakeGenericMethod(typeof(TEvent));

                    _state = createInitStateMethod?.Invoke(EventHandler, new object[] { evt, stateType }); //(object)(EventHandler.CreateInitialState(evt, stateType));
                    _created = true;
                }

                processManagerStateProperty.SetValue(EventHandler, _state);
            }

            return evt;
        }

        private object EventHandler => Context.EventHandler;

        private bool IsProcessManager
        {
            get
            {
                return (EventHandlerType.IsClass || EventHandlerType.IsValueType && !EventHandlerType.IsEnum) &&
                       !EventHandlerType.IsAbstract &&
                       EventHandlerType.IsPublic &&
                       !EventHandlerType.ContainsGenericParameters &&
                       !EventHandlerType.IsDefined<NoProcessManagerAttribute>() &&
                       (EventHandlerType.Name.EndsWith("ProcessManager", StringComparison.OrdinalIgnoreCase) || EventHandlerType.IsDefined<ProcessManagerAttribute>());
            }
        }

        private Type EventHandlerType => Context.EventHandler.GetType();

        public override async Task<TEventResult> PostProcessAsync<TEventResult>(TEventResult eventResult)
        {
            if (!IsProcessManager)
            {
                return eventResult;
            }

            var isTerminatedProperty = EventHandlerType.GetProperty("IsTerminated", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var terminated = isTerminatedProperty != null &&
                             isTerminatedProperty.CanRead &&
                             isTerminatedProperty.PropertyType == typeof(bool) &&
                             isTerminatedProperty.GetIndexParameters().Length == 0 &&
                             (bool)isTerminatedProperty.GetValue(EventHandler);//(bool)EventHandler.IsTerminated;

            if (_created && !terminated)
            {
                ((dynamic)_dataStore).Add(_state);
            }
            else if (!_created && terminated)
            {
                ((dynamic)_dataStore).Remove(_state);
            }
            else if (!_created && !terminated)
            {
                ((dynamic)_dataStore).Update(_state);
            }
            // else if (_created && terminated)
            // {
            //     Nothing to store, because data is not yet in the db and shall not be in the db from now on.
            // }

            await _dataStore.SaveChangesAsync();
            return eventResult;
        }
    }
}
