using System;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace AI4E.Storage.MongoDB
{
    public static class ServiceCollectionExtension
    {
        public static /* TODO */ void AddMongoDBStorage(this IServiceCollection serviceCollection, string connectionString, string db)
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.AddSingleton(provider => new MongoClient(connectionString));
            serviceCollection.AddScoped(provider => provider.GetRequiredService<MongoClient>().GetDatabase(db));
            serviceCollection.AddScoped<IDataStore, MongoDbDataStore>();
        }
    }
}
