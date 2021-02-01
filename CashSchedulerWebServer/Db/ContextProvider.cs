using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace CashSchedulerWebServer.Db
{
    public class ContextProvider : IContextProvider
    {
        private IServiceProvider ServiceProvider { get; }

        public ContextProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }


        public T GetRepository<T>() where T : class
        {
            var repository = ServiceProvider.GetRequiredService<T>();

            if (repository == null)
            {
                throw new CashSchedulerException($"No repositories were registered for the type {typeof(T)}");
            }
            return repository;
        }
    }
}
