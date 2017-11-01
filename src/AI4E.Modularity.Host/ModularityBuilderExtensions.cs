using System;
using Microsoft.Extensions.DependencyInjection;

namespace AI4E.Modularity
{
    public static class ModularityBuilderExtensions
    {
        public static IModularityBuilder Configure(this IModularityBuilder builder, Action<ModularityOptions> configuration)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            builder.Services.Configure(configuration);

            return builder;
        }
    }
}
