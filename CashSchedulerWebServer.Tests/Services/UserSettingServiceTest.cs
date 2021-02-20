﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Services.Settings;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CashSchedulerWebServer.Tests.Services
{
    public class UserSettingServiceTest
    {
        private const int TESTING_USER_ID = 1;
        
        private IUserSettingService UserSettingService { get; }
        private Mock<IUserSettingRepository> UserSettingRepository { get; }
        private Mock<IUserRepository> UserRepository { get; }
        private Mock<IContextProvider> ContextProvider { get; }
        private Mock<IUserContext> UserContext { get; }
        
        public UserSettingServiceTest()
        {
            ContextProvider = new Mock<IContextProvider>();
            UserSettingRepository = new Mock<IUserSettingRepository>();
            UserRepository = new Mock<IUserRepository>();
            UserContext = new Mock<IUserContext>();

            UserContext.Setup(c => c.GetUserId()).Returns(TESTING_USER_ID);

            ContextProvider
                .Setup(c => c.GetRepository<IUserSettingRepository>())
                .Returns(UserSettingRepository.Object);
            
            ContextProvider
                .Setup(c => c.GetRepository<IUserRepository>())
                .Returns(UserRepository.Object);

            UserSettingService = new UserSettingService(ContextProvider.Object, UserContext.Object);
        }


        [Fact]
        public void GetByUnitName_ReturnsAllSettings()
        {
            string userSettingJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"UserSettings.json");
            var userSettings = JsonConvert.DeserializeObject<List<UserSetting>>(userSettingJson).Where(u => u.User.Id == TESTING_USER_ID);

            UserSettingRepository.Setup(s => s.GetAll()).Returns(userSettings);


            var resultUserSettings = UserSettingService.GetByUnitName(null);
            
            
            Assert.NotNull(resultUserSettings);
            Assert.Equal(userSettings.Count(), resultUserSettings.Count());
        }
        
        [Fact]
        public void GetByUnitName_ReturnsGeneralSettings()
        {
            string userSettingJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"UserSettings.json");
            var userSettings = JsonConvert.DeserializeObject<List<UserSetting>>(userSettingJson).Where(u => u.User.Id == TESTING_USER_ID);

            string testingUnitName = UserSetting.UnitOptions.General.ToString();
            
            UserSettingRepository.Setup(s => s.GetAll()).Returns(userSettings);
            
            UserSettingRepository
                .Setup(s => s.GetByUnitName(UserSetting.UnitOptions.General.ToString()))
                .Returns(userSettings.Where(s => s.UnitName == UserSetting.UnitOptions.General.ToString()));
            
            UserSettingRepository
                .Setup(s => s.GetByUnitName(UserSetting.UnitOptions.Notifications.ToString()))
                .Returns(userSettings.Where(s => s.UnitName == UserSetting.UnitOptions.Notifications.ToString()));


            var resultUserSettings = UserSettingService.GetByUnitName(testingUnitName);
            
            
            Assert.NotNull(resultUserSettings);
            Assert.Equal(
                userSettings.Count(s => s.UnitName == UserSetting.UnitOptions.General.ToString()),
                resultUserSettings.Count()
            );
        }
        
        [Fact]
        public async Task Create_ReturnsNewSettings()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);
            
            string settingName = UserSetting.SettingOptions.ShowBalance.ToString();
            string settingUnitName = UserSetting.UnitOptions.General.ToString();
            string settingValue = false.ToString().ToLower();

            var newUserSetting = new UserSetting
            {
                Name = settingName,
                UnitName = settingUnitName,
                Value = settingValue
            };

            UserRepository.Setup(u => u.GetByKey(TESTING_USER_ID)).Returns(user);
            
            UserSettingRepository.Setup(s => s.Create(newUserSetting)).ReturnsAsync(newUserSetting);
            
            
            var resultUserSetting = await UserSettingService.Create(newUserSetting);
            
            
            Assert.NotNull(resultUserSetting);
            Assert.NotNull(resultUserSetting.User);
            Assert.Equal(settingName, resultUserSetting.Name);
            Assert.Equal(settingUnitName, resultUserSetting.UnitName);
            Assert.Equal(settingValue, resultUserSetting.Value);
        }
        
        [Fact]
        public async Task Update_ReturnsNewSettings()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(s => s.Id == TESTING_USER_ID);
            
            string settingName = UserSetting.SettingOptions.ShowBalance.ToString();
            string settingUnitName = UserSetting.UnitOptions.General.ToString();
            string settingValue = true.ToString().ToLower();

            var newUserSetting = new UserSetting
            {
                Name = settingName,
                UnitName = settingUnitName,
                Value = settingValue
            };

            UserRepository.Setup(u => u.GetByKey(TESTING_USER_ID)).Returns(user);
            
            UserSettingRepository.Setup(s => s.GetByName(newUserSetting.Name)).Returns((UserSetting) null);
            
            UserSettingRepository.Setup(s => s.Create(newUserSetting)).ReturnsAsync(newUserSetting);


            var resultUserSetting = await UserSettingService.Update(newUserSetting);
            
            
            Assert.NotNull(resultUserSetting);
            Assert.Equal(settingName, resultUserSetting.Name);
            Assert.Equal(settingUnitName, resultUserSetting.UnitName);
            Assert.Equal(settingValue, resultUserSetting.Value);
        }
        
        [Fact]
        public async Task Update_ReturnsUpdatedSettings()
        {
            string userSettingsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"UserSettings.json");
            var userSetting = JsonConvert.DeserializeObject<List<UserSetting>>(userSettingsJson).First(s => s.User.Id == TESTING_USER_ID);
            
            string settingName = UserSetting.SettingOptions.ShowBalance.ToString();
            string settingUnitName = UserSetting.UnitOptions.General.ToString();
            string settingValue = false.ToString().ToLower();

            var newUserSetting = new UserSetting
            {
                Id = userSetting.Id,
                Name = userSetting.Name,
                UnitName = userSetting.UnitName,
                Value = settingValue
            };

            UserSettingRepository.Setup(s => s.GetByName(newUserSetting.Name)).Returns(userSetting);
            
            UserSettingRepository.Setup(s => s.Update(userSetting)).ReturnsAsync(userSetting);
            
            
            var resultUserSetting = await UserSettingService.Update(newUserSetting);
            
            
            Assert.NotNull(resultUserSetting);
            Assert.Equal(settingName, resultUserSetting.Name);
            Assert.Equal(settingUnitName, resultUserSetting.UnitName);
            Assert.Equal(settingValue, resultUserSetting.Value);
        }

        [Fact]
        public async Task Update_ReturnsUpdatedAndCreatedSettings()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(s => s.Id == TESTING_USER_ID);
            
            string userSettingsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"UserSettings.json");
            var userSetting = JsonConvert.DeserializeObject<List<UserSetting>>(userSettingsJson).First(s => s.User.Id == TESTING_USER_ID);
            
            string settingName = UserSetting.SettingOptions.TurnNotificationsSoundOn.ToString();
            string settingUnitName = UserSetting.UnitOptions.Notifications.ToString();
            string settingValue = true.ToString().ToLower();

            var updatingUserSetting = new UserSetting
            {
                Id = userSetting.Id,
                Name = userSetting.Name,
                UnitName = userSetting.UnitName,
                Value = settingValue,
                User = userSetting.User
            };
            
            var newUserSetting = new UserSetting
            {
                Name = settingName,
                UnitName = settingUnitName,
                Value = settingValue
            };
            
            UserRepository.Setup(u => u.GetByKey(TESTING_USER_ID)).Returns(user);
            
            UserSettingRepository.Setup(s => s.GetByName(newUserSetting.Name)).Returns((UserSetting) null);

            UserSettingRepository.Setup(s => s.GetByName(updatingUserSetting.Name)).Returns(userSetting);
            
            UserSettingRepository
                .Setup(s => s.Create(new List<UserSetting> {newUserSetting}))
                .ReturnsAsync(new List<UserSetting> {newUserSetting});
            
            UserSettingRepository
                .Setup(s => s.Update(new List<UserSetting> {updatingUserSetting}))
                .ReturnsAsync(new List<UserSetting> {updatingUserSetting});
            
            
            var resultUserSettings = await UserSettingService.Update(new List<UserSetting>
            {
                newUserSetting,
                updatingUserSetting
            });
            
            
            Assert.NotNull(resultUserSettings);
            // I can't assert this because inside service it creates a new list so I can't mock repository methods results
            //Assert.Equal(2, resultUserSettings.Count());
            Assert.NotNull(newUserSetting.User);
            Assert.Equal(TESTING_USER_ID, newUserSetting.User.Id);
            Assert.Equal(settingName, newUserSetting.Name);
            Assert.Equal(settingUnitName, newUserSetting.UnitName);
            Assert.Equal(settingValue, newUserSetting.Value);
            Assert.NotNull(userSetting.User);
            Assert.Equal(TESTING_USER_ID, userSetting.User.Id);
            Assert.Equal(updatingUserSetting.Name, userSetting.Name);
            Assert.Equal(updatingUserSetting.UnitName, userSetting.UnitName);
            Assert.Equal(updatingUserSetting.Value, userSetting.Value);
        }

        [Fact]
        public async Task Delete_ReturnsDeletedSetting()
        {
            string userSettingsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"UserSettings.json");
            var userSetting = JsonConvert.DeserializeObject<List<UserSetting>>(userSettingsJson).First(s => s.User.Id == TESTING_USER_ID);
            
            UserSettingRepository.Setup(s => s.GetByKey(userSetting.Id)).Returns(userSetting);
            
            UserSettingRepository.Setup(s => s.Delete(userSetting.Id)).ReturnsAsync(userSetting);


            var resultUserSetting = await UserSettingService.Delete(userSetting.Id);
            
            
            Assert.NotNull(resultUserSetting);
            Assert.Equal(userSetting.Name, resultUserSetting.Name);
            Assert.Equal(userSetting.UnitName, resultUserSetting.UnitName);
            Assert.Equal(userSetting.Value, resultUserSetting.Value);
        }
    }
}
