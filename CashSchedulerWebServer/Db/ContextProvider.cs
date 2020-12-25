using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using GraphQL;

namespace CashSchedulerWebServer.Db
{
    public class ContextProvider : IContextProvider
    {
        private readonly IDependencyResolver DependencyResolver;

        public ContextProvider(IDependencyResolver dependencyResolver)
        {
            DependencyResolver = dependencyResolver;
        }


        public T GetRepository<T>() where T : class
        {
            var repository = DependencyResolver.Resolve<T>();

            if (repository == null)
            {
                throw new CashSchedulerException($"No repositories were registered for the type {typeof(T)}");
            }
            return repository;
        }
    }
}
