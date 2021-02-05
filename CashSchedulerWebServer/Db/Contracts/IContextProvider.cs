namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IContextProvider
    {
        T GetRepository<T>() where T : class;
    }
}
