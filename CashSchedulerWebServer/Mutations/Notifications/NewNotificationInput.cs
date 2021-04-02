namespace CashSchedulerWebServer.Mutations.Notifications
{
    public class NewNotificationInput
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public int UserId { get; set; }
    }
}
