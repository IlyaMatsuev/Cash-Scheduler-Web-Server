using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Services.Notifications;
using HotChocolate.Subscriptions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CashSchedulerWebServer.Tests.Services
{
    public class UserNotificationServiceTest
    {
        private const int TESTING_USER_ID = 1;
        
        private IUserNotificationService UserNotificationService { get; }
        private Mock<IUserNotificationRepository> UserNotificationRepository { get; }
        private Mock<IUserRepository> UserRepository { get; }
        private Mock<IContextProvider> ContextProvider { get; }
        private Mock<IUserContext> UserContext { get; }
        private Mock<ITopicEventSender> TopicEventSender { get; }

        public UserNotificationServiceTest()
        {
            ContextProvider = new Mock<IContextProvider>();
            UserNotificationRepository = new Mock<IUserNotificationRepository>();
            UserRepository = new Mock<IUserRepository>();
            UserContext = new Mock<IUserContext>();
            TopicEventSender = new Mock<ITopicEventSender>();

            UserContext.Setup(c => c.GetUserId()).Returns(TESTING_USER_ID);

            ContextProvider
                .Setup(c => c.GetRepository<IUserNotificationRepository>())
                .Returns(UserNotificationRepository.Object);
            
            ContextProvider
                .Setup(c => c.GetRepository<IUserRepository>())
                .Returns(UserRepository.Object);

            UserNotificationService = new UserNotificationService(
                ContextProvider.Object, 
                UserContext.Object, 
                TopicEventSender.Object
            );
        }

        [Fact]
        public void GetUnreadCount_ReturnsCount()
        {
            string userNotificationsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"UserNotifications.json");
            var notifications = JsonConvert.DeserializeObject<List<UserNotification>>(userNotificationsJson)
                .Where(n => n.User.Id == TESTING_USER_ID && !n.IsRead);

            UserNotificationRepository.Setup(n => n.GetUnread()).Returns(notifications);


            int unreadNotificationsCount = UserNotificationService.GetUnreadCount();


            Assert.Equal(notifications.Count(), unreadNotificationsCount);
        }

        [Fact]
        public async Task Create_ReturnsNewNotification()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            const string title = "Test title";
            const string content = "Test content";

            var userNotification = new UserNotification
            {
                Title = title,
                Content = content,
                IsRead = false
            };

            UserRepository.Setup(u => u.GetByKey(TESTING_USER_ID)).Returns(user);

            UserNotificationRepository.Setup(n => n.Create(userNotification)).ReturnsAsync(userNotification);

            
            var resultUserNotification = await UserNotificationService.Create(userNotification);


            Assert.NotNull(resultUserNotification);
            Assert.NotNull(resultUserNotification.User);
            Assert.Equal(title, resultUserNotification.Title);
            Assert.Equal(content, resultUserNotification.Content);
            Assert.Equal(DateTime.Today, resultUserNotification.CreatedDate);
            Assert.False(resultUserNotification.IsRead);
        }
        
        [Fact]
        public async Task Update_ReturnsUpdatedNotification()
        {
            string userNotificationsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"UserNotifications.json");
            var notification = JsonConvert.DeserializeObject<List<UserNotification>>(userNotificationsJson).First(u => u.User.Id == TESTING_USER_ID);

            const string newTitle = "Test title";
            const string newContent = "Test content";

            var userNotification = new UserNotification
            {
                Id = notification.Id,
                Title = newTitle,
                Content = newContent,
                IsRead = true
            };

            UserNotificationRepository.Setup(u => u.GetByKey(userNotification.Id)).Returns(notification);

            UserNotificationRepository.Setup(n => n.Update(notification)).ReturnsAsync(notification);


            var resultUserNotification = await UserNotificationService.Update(userNotification);


            Assert.NotNull(resultUserNotification);
            Assert.Equal(newTitle, resultUserNotification.Title);
            Assert.Equal(newContent, resultUserNotification.Content);
            Assert.Equal(notification.CreatedDate, resultUserNotification.CreatedDate);
            Assert.True(resultUserNotification.IsRead);
        }
        
        [Fact]
        public async Task Delete_ReturnsDeletedNotification()
        {
            string userNotificationsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"UserNotifications.json");
            var notification = JsonConvert.DeserializeObject<List<UserNotification>>(userNotificationsJson).First(u => u.User.Id == TESTING_USER_ID);

            UserNotificationRepository.Setup(u => u.GetByKey(notification.Id)).Returns(notification);

            UserNotificationRepository.Setup(n => n.Delete(notification.Id)).ReturnsAsync(notification);


            var resultUserNotification = await UserNotificationService.Delete(notification.Id);


            Assert.NotNull(resultUserNotification);
            Assert.Equal(notification.Title, resultUserNotification.Title);
            Assert.Equal(notification.Content, resultUserNotification.Content);
            Assert.Equal(notification.IsRead, resultUserNotification.IsRead);
            Assert.Equal(notification.CreatedDate, resultUserNotification.CreatedDate);
        }
    }
}
