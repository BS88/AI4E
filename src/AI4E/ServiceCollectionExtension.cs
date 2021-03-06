﻿/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        ServiceCollectionExtension.cs 
 * Types:           AI4E.ServiceCollectionExtension
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   04.09.2017 
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

/* Based on
 * --------------------------------------------------------------------------------------------------------------------
 * Asp.Net Core MVC
 * Copyright (c) .NET Foundation. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use
 * these files except in compliance with the License. You may obtain a copy of the
 * License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed
 * under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 * --------------------------------------------------------------------------------------------------------------------
 */

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AI4E.Integration;
using AI4E.Storage;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AI4E
{
    public static class ServiceCollectionExtension
    {
        #region Messaging

        public static IMessagingBuilder AddMessaging(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Configure necessary application parts
            var partManager = services.GetApplicationPartManager();
            partManager.ConfigureMessagingFeatureProviders();
            services.TryAddSingleton(partManager);

            // Configure services
            services.AddSingleton(provider => BuildCommandDispatcher(provider, services.GetService<ICommandDispatcher>()));
            services.AddSingleton(provider => BuildQueryDispatcher(provider, services.GetService<IQueryDispatcher>()));
            services.AddSingleton(provider => BuildEventDispatcher(provider, services.GetService<IEventDispatcher>()));

            services.AddSingleton<INonGenericCommandDispatcher, ICommandDispatcher>(provider => provider.GetRequiredService<ICommandDispatcher>());
            services.AddSingleton<INonGenericQueryDispatcher, IQueryDispatcher>(provider => provider.GetRequiredService<IQueryDispatcher>());
            services.AddSingleton<INonGenericEventDispatcher, IEventDispatcher>(provider => provider.GetRequiredService<IEventDispatcher>());

            return new MessagingBuilder(services);
        }

        public static IMessagingBuilder AddMessaging(this IServiceCollection services, Action<MessagingOptions> configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var builder = services.AddMessaging();
            builder.Services.Configure(configuration);
            return builder;
        }

        private static ICommandDispatcher BuildCommandDispatcher(IServiceProvider serviceProvider, ICommandDispatcher commandDispatcher)
        {
            if (commandDispatcher == null)
            {
                commandDispatcher = new CommandDispatcher(serviceProvider);
            }

            var partManager = serviceProvider.GetRequiredService<ApplicationPartManager>();
            var commandHandlerFeature = new CommandHandlerFeature();

            partManager.PopulateFeature(commandHandlerFeature);

            foreach (var type in commandHandlerFeature.CommandHandlers)
            {
                var commandHandlerInspector = new CommandHandlerInspector(type);
                var commandHandlerDescriptors = commandHandlerInspector.GetCommandHandlerDescriptors();

                foreach (var commandHandlerDescriptor in commandHandlerDescriptors)
                {
                    var commandType = commandHandlerDescriptor.CommandType;
                    var commandHandlerProvider = Activator.CreateInstance(typeof(CommandHandlerProvider<>).MakeGenericType(commandType), type, commandHandlerDescriptor);

                    Task taskRegistration = commandDispatcher.RegisterAsync((dynamic)commandHandlerProvider); // TODO: The task is neither awaited nor stored.
                }
            }

            return commandDispatcher;
        }

        private static IQueryDispatcher BuildQueryDispatcher(IServiceProvider serviceProvider, IQueryDispatcher queryDispatcher)
        {
            if (queryDispatcher == null)
            {
                queryDispatcher = new QueryDispatcher(serviceProvider);
            }

            var partManager = serviceProvider.GetRequiredService<ApplicationPartManager>();
            var queryHandlerFeature = new QueryHandlerFeature();

            partManager.PopulateFeature(queryHandlerFeature);

            foreach (var type in queryHandlerFeature.QueryHandlers)
            {
                var queryHandlerInspector = new QueryHandlerInspector(type);
                var queryHandlerDescriptors = queryHandlerInspector.GetQueryHandlerDescriptors();

                foreach (var queryHandlerDescriptor in queryHandlerDescriptors)
                {
                    var queryType = queryHandlerDescriptor.QueryType;
                    var queryHandlerProvider = Activator.CreateInstance(typeof(QueryHandlerProvider<>).MakeGenericType(queryType), type, queryHandlerDescriptor);

                    Task taskRegistration = queryDispatcher.RegisterAsync((dynamic)queryHandlerProvider); // TODO: The task is neither awaited nor stored.
                }
            }

            return queryDispatcher;
        }

        private static IEventDispatcher BuildEventDispatcher(IServiceProvider serviceProvider, IEventDispatcher eventDispatcher)
        {
            if (eventDispatcher == null)
            {
                eventDispatcher = new EventDispatcher(serviceProvider);
            }

            var partManager = serviceProvider.GetRequiredService<ApplicationPartManager>();
            var eventHandlerFeature = new EventHandlerFeature();

            partManager.PopulateFeature(eventHandlerFeature);

            foreach (var type in eventHandlerFeature.EventHandlers)
            {
                var eventHandlerInspector = new EventHandlerInspector(type);
                var eventHandlerDescriptors = eventHandlerInspector.GetEventHandlerDescriptors();

                foreach (var eventHandlerDescriptor in eventHandlerDescriptors)
                {
                    var eventType = eventHandlerDescriptor.EventType;
                    var eventHandlerProvider = Activator.CreateInstance(typeof(EventHandlerProvider<>).MakeGenericType(eventType), type, eventHandlerDescriptor);

                    Task taskRegistration = eventDispatcher.RegisterAsync((dynamic)eventHandlerProvider); // TODO: The task is neither awaited nor stored.
                }
            }

            return eventDispatcher;
        }

        private static void ConfigureMessagingFeatureProviders(this ApplicationPartManager partManager)
        {
            if (!partManager.FeatureProviders.OfType<CommandHandlerFeatureProvider>().Any())
            {
                partManager.FeatureProviders.Add(new CommandHandlerFeatureProvider());
            }

            if (!partManager.FeatureProviders.OfType<QueryHandlerFeatureProvider>().Any())
            {
                partManager.FeatureProviders.Add(new QueryHandlerFeatureProvider());
            }

            if (!partManager.FeatureProviders.OfType<EventHandlerFeatureProvider>().Any())
            {
                partManager.FeatureProviders.Add(new EventHandlerFeatureProvider());
            }
        }

        #endregion

        #region EventSourcing

        public static IEventSourcingBuilder AddEventSourcing(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Configure necessary application parts
            var partManager = services.GetApplicationPartManager();
            partManager.ConfigureEventSourcingFeatureProvider();
            services.TryAddSingleton(partManager);

            // Configure services
            // TODO

            return new EventSourcingBuilder(services);
        }

        public static IEventSourcingBuilder AddEventSourcing(this IServiceCollection services, Action<EventSourcingOptions> configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var builder = AddEventSourcing(services);
            builder.Configure(configuration);
            return builder;
        }

        private static void ConfigureEventSourcingFeatureProvider(this ApplicationPartManager partManager)
        {
            if (!partManager.FeatureProviders.OfType<EventReplayerFeatureProvider>().Any())
            {
                partManager.FeatureProviders.Add(new EventReplayerFeatureProvider());
            }
        }

        #endregion

        private static ApplicationPartManager GetApplicationPartManager(this IServiceCollection services)
        {
            var manager = services.GetService<ApplicationPartManager>();
            if (manager == null)
            {
                manager = new ApplicationPartManager();
                var parts = DefaultAssemblyPartDiscoveryProvider.DiscoverAssemblyParts(Assembly.GetEntryAssembly().FullName);
                foreach (var part in parts)
                {
                    manager.ApplicationParts.Add(part);
                }
            }

            return manager;
        }

        private static T GetService<T>(this IServiceCollection services)
        {
            return (T)services
                .LastOrDefault(d => d.ServiceType == typeof(T))
                ?.ImplementationInstance;
        }
    }
}
