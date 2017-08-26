/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        AI4EServiceCollectionExtension.cs 
 * Types:           AI4E.AI4EServiceCollectionExtension
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   26.08.2017 
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AI4E.Integration;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AI4E
{
    public static class AI4EServiceCollectionExtension
    {
        public static IMessagingBuilder AddMessaging(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var partManager = GetApplicationPartManager(services);
            Debug.Assert(partManager != null);

            ConfigureDefaultFeatureProviders(partManager);

            services.TryAddSingleton(partManager);

            services.AddSingleton(BuildCommandDispatcher);
            services.AddSingleton<QueryDispatcher>();
            services.AddSingleton<EventDispatcher>();

            services.AddSingleton<IQueryDispatcher, QueryDispatcher>(provider => provider.GetRequiredService<QueryDispatcher>());
            services.AddSingleton<IEventDispatcher, EventDispatcher>(provider => provider.GetRequiredService<EventDispatcher>());

            services.AddSingleton<INonGenericCommandDispatcher, ICommandDispatcher>(provider => provider.GetRequiredService<ICommandDispatcher>());
            services.AddSingleton<INonGenericQueryDispatcher, QueryDispatcher>(provider => provider.GetRequiredService<QueryDispatcher>());
            services.AddSingleton<INonGenericEventDispatcher, EventDispatcher>(provider => provider.GetRequiredService<EventDispatcher>());

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

        private static ICommandDispatcher BuildCommandDispatcher(IServiceProvider serviceProvider)
        {
            var result = new CommandDispatcher(serviceProvider);

            var applicationPartManager = serviceProvider.GetRequiredService<ApplicationPartManager>();
            var commandHandlerFeature = new CommandHandlerFeature();

            applicationPartManager.PopulateFeature(commandHandlerFeature);

            foreach (var type in commandHandlerFeature.CommandHandlers)
            {
                var commandHandlerInspector = new CommandHandlerInspector(type);
                var commandHandlerDescriptors = commandHandlerInspector.GetCommandHandlerDescriptors();

                foreach (var commandHandlerDescriptor in commandHandlerDescriptors)
                {
                    var commandType = commandHandlerDescriptor.CommandType;
                    var commandHandlerProvider = Activator.CreateInstance(typeof(CommandHandlerProvider<>).MakeGenericType(commandType), type, commandHandlerDescriptor);

                    result.RegisterAsync((dynamic)commandHandlerProvider);
                }
            }

            return result;
        }

        private static void ConfigureDefaultFeatureProviders(ApplicationPartManager manager)
        {
            if (!manager.FeatureProviders.OfType<CommandHandlerFeatureProvider>().Any())
            {
                manager.FeatureProviders.Add(new CommandHandlerFeatureProvider());
            }
        }

        private static ApplicationPartManager GetApplicationPartManager(IServiceCollection services)
        {
            var manager = GetServiceFromCollection<ApplicationPartManager>(services);
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

        private static T GetServiceFromCollection<T>(IServiceCollection services)
        {
            return (T)services
                .LastOrDefault(d => d.ServiceType == typeof(T))
                ?.ImplementationInstance;
        }
    }
}
