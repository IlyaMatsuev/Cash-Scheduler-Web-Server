using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace CashSchedulerWebServer.Db
{
    public class ContextProvider : IContextProvider
    {
        // ContextProvider should be TRANSIENT service
        // DbContext should be TRANSIENT service
        // Old ContextProvider - https://github.com/IlyaMatsuev/Cash-Scheduler-Web-Server/blob/master/CashSchedulerWebServer/Db/ContextProvider.cs
        
        /*
         * 1. Return instances of repositories NOT FROM ASP.NET DI but created myself
         * 2. Pass particular arguments to repositories
         * 3. For Services try to pass as IContextProvider THIS instance
         */
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

        public T GetService<T>()
        {
            var repository = ServiceProvider.GetService<T>();

            if (repository == null)
            {
                throw new CashSchedulerException($"No repositories were registered for the type {typeof(T)}");
            }
            return repository;
        }
    }
}
