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
    public class ReportingJob : IJob
    {
        private readonly CashSchedulerContext cashSchedulerContext;
        private readonly INotificator notificator;

        public ReportingJob(CashSchedulerContext cashSchedulerContext, INotificator notificator)
        {
            this.cashSchedulerContext = cashSchedulerContext;
            this.notificator = notificator;
        }


        public Task Execute(IJobExecutionContext context)
        {
            var now = DateTime.UtcNow;
            Console.WriteLine($"Running the {context.JobDetail.Description}");
            var transactionsByUsers = cashSchedulerContext.Transactions.Where(t => t.Date >= now.AddDays(-7).Date && t.TransactionCategory.Type.Name == TransactionType.Options.Expense.ToString())
                .Include(t => t.TransactionCategory)
                .Include(t => t.TransactionCategory.Type)
                .Include(t => t.CreatedBy).ToList()
                .GroupBy(t => t.CreatedBy);

            var usersByMaxSpentCategory = transactionsByUsers.Select(group =>
            {
                var maxSpentCategoryForLastWeek = group.GroupBy(t => t.TransactionCategory).OrderBy(c => c.Max(t => c.Sum(tc => tc.Amount)))
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


            using (var dmlTransaction = cashSchedulerContext.Database.BeginTransaction())
            {
                try
                {
                    var notifications = NotifyUsers(usersByMaxSpentCategory);

                    cashSchedulerContext.UserNotifications.AddRange(notifications);
                    cashSchedulerContext.SaveChanges();

                    dmlTransaction.Commit();

                    Console.WriteLine($"{notifications.Count()} users were notified");
                }
                catch (Exception error)
                {
                    dmlTransaction.Rollback();
                    Console.WriteLine($"Error while running the {context.JobDetail.Description}: {error.Message}: \n{error.StackTrace}");
                }

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
                List<UserSetting> settings = settingsByUsers.FirstOrDefault(s => s.Key == user)?.ToList();
                if (settings != null)
                {
                    UserSetting notificationsEnabledSetting = settings.FirstOrDefault(s => s.Name == UserSetting.SettingOptions.TurnNotificationsOn.ToString());
                    UserSetting duplicateToEmailSetting = settings.FirstOrDefault(s => s.Name == UserSetting.SettingOptions.DuplicateToEmail.ToString());

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
                            CreatedFor = user
                        };
                        notificationsToBeCreated.Add(notification);
                        if (duplicateToEmailSetting?.Value == "true")
                        {
                            notificator.SendEmail(user.Email, notificationTemplate);
                        }
                    }
                }
            });

            return notificationsToBeCreated;
        }

        private List<IGrouping<User, UserSetting>> GetSettingsByUsers(List<int> usersIds)
        {
            return cashSchedulerContext.UserSettings
                .Where(s => usersIds.Contains(s.SettingFor.Id) && s.UnitName == UserSetting.UnitOptions.Notifications.ToString()).AsEnumerable()
                .GroupBy(s => s.SettingFor).ToList();
        }
    }
}
