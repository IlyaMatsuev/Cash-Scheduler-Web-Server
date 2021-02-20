using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Services.Users;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CashSchedulerWebServer.Tests.Services
{
    public class UserEmailVerificationCodeServiceTest
    {
        private const int TESTING_USER_ID = 1;
        
        private IUserEmailVerificationCodeService UserEmailVerificationCodeService { get; }
        private Mock<IUserEmailVerificationCodeRepository> UserEmailVerificationCodeRepository { get; }
        private Mock<IContextProvider> ContextProvider { get; }

        public UserEmailVerificationCodeServiceTest()
        {
            UserEmailVerificationCodeRepository = new Mock<IUserEmailVerificationCodeRepository>();
            ContextProvider = new Mock<IContextProvider>();

            ContextProvider
                .Setup(c => c.GetRepository<IUserEmailVerificationCodeRepository>())
                .Returns(UserEmailVerificationCodeRepository.Object);

            UserEmailVerificationCodeService = new UserEmailVerificationCodeService(ContextProvider.Object);
        }
        
        
        [Fact]
        public async Task Update_ReturnsNewCode()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            const string newCode = "1232123";
            DateTime newExpirationDate = DateTime.Now.AddMinutes(5);

            var newVerificationCode = new UserEmailVerificationCode
            {
                User = user,
                Code = newCode,
                ExpiredDate = newExpirationDate
            };

            UserEmailVerificationCodeRepository
                .Setup(e => e.GetByUserId(TESTING_USER_ID))
                .Returns((UserEmailVerificationCode) null);
            
            UserEmailVerificationCodeRepository
                .Setup(r => r.Create(newVerificationCode))
                .ReturnsAsync(newVerificationCode);

            
            var resultToken = await UserEmailVerificationCodeService.Update(newVerificationCode);
            
            
            Assert.NotNull(resultToken);
            Assert.NotNull(resultToken.User);
            Assert.Equal(TESTING_USER_ID, resultToken.User.Id);
            Assert.Equal(newCode, resultToken.Code);
            Assert.Equal(newExpirationDate, resultToken.ExpiredDate);
        }
        
        [Fact]
        public async Task Update_ReturnsUpdatedCode()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            const string newCode = "1232123";
            DateTime newExpirationDate = DateTime.Now.AddMinutes(5);
            
            var newVerificationCode = new UserEmailVerificationCode
            {
                Id = 1,
                User = user,
                Code = newCode,
                ExpiredDate = newExpirationDate
            };

            UserEmailVerificationCodeRepository
                .Setup(r => r.GetByUserId(TESTING_USER_ID))
                .Returns(newVerificationCode);

            UserEmailVerificationCodeRepository
                .Setup(r => r.Update(newVerificationCode))
                .ReturnsAsync(newVerificationCode);

            
            var resultToken = await UserEmailVerificationCodeService.Update(newVerificationCode);
            
            
            Assert.NotNull(resultToken);
            Assert.NotNull(resultToken.User);
            Assert.Equal(TESTING_USER_ID, resultToken.User.Id);
            Assert.Equal(newCode, resultToken.Code);
            Assert.Equal(newExpirationDate, resultToken.ExpiredDate);
        }
        
        [Fact]
        public async Task Delete_ReturnsDeletedCode()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            const int verificationCodeId = 2;
            const string newCode = "1232123";
            DateTime expirationDate = DateTime.Now.AddMinutes(5);
            
            var newVerificationCode = new UserEmailVerificationCode
            {
                Id = verificationCodeId,
                User = user,
                Code = newCode,
                ExpiredDate = expirationDate
            };

            UserEmailVerificationCodeRepository
                .Setup(r => r.GetByKey(newVerificationCode.Id))
                .Returns(newVerificationCode);

            UserEmailVerificationCodeRepository
                .Setup(r => r.Delete(newVerificationCode.Id))
                .ReturnsAsync(newVerificationCode);

            
            var resultToken = await UserEmailVerificationCodeService.Delete(newVerificationCode.Id);
            
            
            Assert.NotNull(resultToken);
            Assert.NotNull(resultToken.User);
            Assert.Equal(TESTING_USER_ID, resultToken.User.Id);
            Assert.Equal(newCode, resultToken.Code);
            Assert.Equal(expirationDate, resultToken.ExpiredDate);
        }
    }
}
