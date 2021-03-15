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
    public class UserRefreshTokenServiceTest
    {
        private const int TESTING_USER_ID = 1;

        private IUserRefreshTokenService UserRefreshTokenService { get; }
        private Mock<IUserRefreshTokenRepository> UserRefreshTokenRepository { get; }
        private Mock<IContextProvider> ContextProvider { get; }

        public UserRefreshTokenServiceTest()
        {
            UserRefreshTokenRepository = new Mock<IUserRefreshTokenRepository>();
            ContextProvider = new Mock<IContextProvider>();

            ContextProvider
                .Setup(c => c.GetRepository<IUserRefreshTokenRepository>())
                .Returns(UserRefreshTokenRepository.Object);

            UserRefreshTokenService = new UserRefreshTokenService(ContextProvider.Object);
        }


        [Fact]
        public async Task Update_ReturnsUpdatedToken()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            const string newToken = "qqqqqqqqqqqqqqqqqqqqqqqwwwwwwwee";
            DateTime newExpirationDate = DateTime.Now.AddMinutes(5);

            var newRefreshToken = new UserRefreshToken
            {
                Id = 1,
                User = user,
                Token = newToken,
                ExpiredDate = newExpirationDate
            };

            UserRefreshTokenRepository.Setup(r => r.GetByKey(TESTING_USER_ID)).Returns(newRefreshToken);

            UserRefreshTokenRepository.Setup(r => r.Update(newRefreshToken)).ReturnsAsync(newRefreshToken);


            var resultToken = await UserRefreshTokenService.Update(newRefreshToken);


            Assert.NotNull(resultToken);
            Assert.NotNull(resultToken.User);
            Assert.Equal(TESTING_USER_ID, resultToken.User.Id);
            Assert.Equal(newToken, resultToken.Token);
            Assert.Equal(newExpirationDate, resultToken.ExpiredDate);
        }

        [Fact]
        public async Task Delete_ReturnsDeletedToken()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            const int refreshTokenId = 2;
            const string token = "qqqqqqqqqqqqqqqqqqqqqqqwwwwwwwee";
            DateTime expirationDate = DateTime.Now.AddMinutes(5);

            var newRefreshToken = new UserRefreshToken
            {
                Id = refreshTokenId,
                User = user,
                Token = token,
                ExpiredDate = expirationDate
            };

            UserRefreshTokenRepository.Setup(r => r.GetByKey(newRefreshToken.Id)).Returns(newRefreshToken);

            UserRefreshTokenRepository.Setup(r => r.Delete(newRefreshToken.Id)).ReturnsAsync(newRefreshToken);


            var resultToken = await UserRefreshTokenService.Delete(newRefreshToken.Id);


            Assert.NotNull(resultToken);
            Assert.NotNull(resultToken.User);
            Assert.Equal(TESTING_USER_ID, resultToken.User.Id);
            Assert.Equal(token, resultToken.Token);
            Assert.Equal(expirationDate, resultToken.ExpiredDate);
        }
    }
}
