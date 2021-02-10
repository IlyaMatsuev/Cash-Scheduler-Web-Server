namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IContextProvider
    {
        T GetRepository<T>() where T : class;
        T GetService<T>() where T : class;
    }
}
