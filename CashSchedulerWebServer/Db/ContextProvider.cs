using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using GraphQL.Utilities;
using System;

namespace CashSchedulerWebServer.Db
{
    public class ContextProvider : IContextProvider
    {
        private readonly IServiceProvider Provider;

        public ContextProvider(IServiceProvider provider)
        {
            Provider = provider;
        }


        public T GetRepository<T>() where T : class
        {
            var repository = Provider.GetRequiredService<T>();

            if (repository == null)
            {
                throw new CashSchedulerException($"No repositories were registered for the type {typeof(T)}");
            }
            return repository;
        }
    }
}
