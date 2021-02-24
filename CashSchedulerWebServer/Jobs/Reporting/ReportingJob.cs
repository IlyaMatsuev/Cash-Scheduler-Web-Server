using CashSchedulerWebServer.Db;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Notifications;
using CashSchedulerWebServer.Notifications.Contracts;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Jobs.Reporting
{
    [Obsolete]
    public class ReportingJob : IJob
    {
        private CashSchedulerContext CashSchedulerContext { get; }
        private INotificator Notificator { get; }

        public ReportingJob(CashSchedulerContext cashSchedulerContext, INotificator notificator)
        {
            CashSchedulerContext = cashSchedulerContext;
            Notificator = notificator;
        }


        public Task Execute(IJobExecutionContext context)
        {
            var now = DateTime.UtcNow;
            Console.WriteLine($"Running the {context.JobDetail.Description}");
            var transactionsByUsers = CashSchedulerContext.Transactions
                .Where(t => t.Date >= now.AddDays(-7).Date && t.Category.Type.Name == TransactionType.Options.Expense.ToString())
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.User).ToList()
                .GroupBy(t => t.User);

            var usersByMaxSpentCategory = transactionsByUsers.Select(group =>
            {
                var maxSpentCategoryForLastWeek = group.GroupBy(t => t.Category).OrderBy(c => c.Max(_ => c.Sum(tc => tc.Amount)))
                .Select(c => new
                {
                    Category = c.Key,
                    Sum = c.Sum(t => t.Amount)
                }).First();

                return (dynamic)new
                {
                    User = group.Key,
                    MaxSpentCategory = maxSpentCategoryForLastWeek
                };
            }).ToList();


            using var dmlTransaction = CashSchedulerContext.Database.BeginTransaction();
            try
            {
                var notifications = NotifyUsers(usersByMaxSpentCategory);

                CashSchedulerContext.UserNotifications.AddRange(notifications);
                CashSchedulerContext.SaveChanges();

                dmlTransaction.Commit();

                Console.WriteLine($"{notifications.Count} users were notified");
            }
            catch (Exception error)
            {
                dmlTransaction.Rollback();
                Console.WriteLine($"Error while running the {context.JobDetail.Description}: {error.Message}: \n{error.StackTrace}");
            }

            return Task.CompletedTask;
        }


        private List<UserNotification> NotifyUsers(List<dynamic> usersEntries)
        {
            var users = usersEntries.Select(u => u.User).Cast<User>().ToList();
            var settingsByUsers = GetSettingsByUsers(users.Select(u => u.Id).ToList());
            var notificationsToBeCreated = new List<UserNotification>();
            usersEntries.ForEach(entry =>
            {
                var user = entry.User as User;
                var settings = settingsByUsers.FirstOrDefault(s => s.Key == user)?.ToList();
                if (settings != null)
                {
                    var notificationsEnabledSetting = settings.FirstOrDefault(s => s.Name == Setting.SettingOptions.TurnNotificationsOn.ToString());
                    var duplicateToEmailSetting = settings.FirstOrDefault(s => s.Name == Setting.SettingOptions.DuplicateToEmail.ToString());

                    if (notificationsEnabledSetting?.Value == "true")
                    {
                        var notificationParameters = new Dictionary<string, string>
                        {
                            { "category", entry.MaxSpentCategory?.Category.Name as string },
                            { "amount", Convert.ToString(entry.MaxSpentCategory?.Sum) }
                        };
                        var notificationTemplate = new NotificationDelegator().GetTemplate(NotificationTemplateType.MostSpentCategoryForWeek, notificationParameters);
                        var notification = new UserNotification
                        {
                            Title = notificationTemplate.Subject,
                            Content = notificationTemplate.Body,
                            IsRead = false,
                            User = user
                        };
                        notificationsToBeCreated.Add(notification);
                        if (duplicateToEmailSetting?.Value == "true")
                        {
                            Notificator.SendEmail(user?.Email, notificationTemplate);
                        }
                    }
                }
            });

            return notificationsToBeCreated;
        }

        private List<IGrouping<User, UserSetting>> GetSettingsByUsers(List<int> usersIds)
        {
            return CashSchedulerContext.UserSettings
                .Where(s => usersIds.Contains(s.User.Id) && s.Setting.UnitName == Setting.UnitOptions.Notifications.ToString())
                .AsEnumerable()
                .GroupBy(s => s.User).ToList();
        }
    }
}
