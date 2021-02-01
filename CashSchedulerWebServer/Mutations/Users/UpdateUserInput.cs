namespace CashSchedulerWebServer.Mutations.Users
{
    public class UpdateUserInput
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public double? Balance { get; set; }
    }
}
