using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Services.TransactionTypes;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CashSchedulerWebServer.Tests.Services
{
    public class TransactionTypeServiceTest
    {
        private ITransactionTypeService TransactionTypeService { get; }
        private Mock<ITransactionTypeRepository> TransactionTypeRepository { get; }
        private Mock<IContextProvider> ContextProvider { get; }
        
        public TransactionTypeServiceTest()
        {
            ContextProvider = new Mock<IContextProvider>();
            TransactionTypeRepository = new Mock<ITransactionTypeRepository>();
            
            ContextProvider
                .Setup(c => c.GetRepository<ITransactionTypeRepository>())
                .Returns(TransactionTypeRepository.Object);

            TransactionTypeService = new TransactionTypeService(ContextProvider.Object);
        }
        
        
        [Fact]
        public void GetAll_ReturnsAll()
        {
            string expectedTransactionTypesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"TransactionTypes.json");
            var expectedTransactionTypes = JsonConvert.DeserializeObject<List<TransactionType>>(expectedTransactionTypesJson);
            
            TransactionTypeRepository.Setup(r => r.GetAll()).Returns(expectedTransactionTypes);
            
            
            var actualTransactionTypes = TransactionTypeService.GetAll();
            
            
            Assert.NotNull(actualTransactionTypes);
            Assert.Equal(expectedTransactionTypes.Count, actualTransactionTypes.Count());
        }
        
        [Fact]
        public void GetByKey_ReturnsIncomeAndExpense()
        {
            string expectedTransactionTypesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"TransactionTypes.json");
            var expectedTransactionTypes = JsonConvert.DeserializeObject<List<TransactionType>>(expectedTransactionTypesJson);

            var expectedIncome = expectedTransactionTypes
                .First(t => t.Name == TransactionType.Options.Income.ToString());
            var expectedExpense = expectedTransactionTypes
                .First(t => t.Name == TransactionType.Options.Expense.ToString());
            
            TransactionTypeRepository
                .Setup(r => r.GetByKey(TransactionType.Options.Income.ToString())).Returns(expectedIncome);
            
            TransactionTypeRepository
                .Setup(r => r.GetByKey(TransactionType.Options.Expense.ToString())).Returns(expectedExpense);
            
            
            var actualIncome = TransactionTypeService.GetByKey(TransactionType.Options.Income.ToString());
            var actualExpense = TransactionTypeService.GetByKey(TransactionType.Options.Expense.ToString());
            
            
            Assert.NotNull(actualIncome);
            Assert.NotNull(actualExpense);
            
            Assert.Equal(actualIncome.Name, expectedIncome.Name);
            Assert.Equal(actualIncome.IconUrl, expectedIncome.IconUrl);
            
            Assert.Equal(actualExpense.Name, expectedExpense.Name);
            Assert.Equal(actualExpense.IconUrl, expectedExpense.IconUrl);
        }
        
        [Fact]
        public async Task Create_ThrowsException()
        {
            var sampleTransactionType = new TransactionType
            {
                Name = "Debt",
                IconUrl = "/static/icons/transactiontypes/debt.png"
            };

            await Assert.ThrowsAsync<CashSchedulerException>(() => TransactionTypeService.Create(sampleTransactionType));
        }
        
        [Fact]
        public async Task Update_ThrowsException()
        {
            var sampleTransactionType = new TransactionType
            {
                Name = "Income",
                IconUrl = "/static/icons/transactiontypes/income-new.png"
            };

            await Assert.ThrowsAsync<CashSchedulerException>(() => TransactionTypeService.Update(sampleTransactionType));
        }
        
        [Fact]
        public async Task Delete_ThrowsException()
        {
            await Assert.ThrowsAsync<CashSchedulerException>(() => 
                TransactionTypeService.Delete(TransactionType.Options.Income.ToString()));
            
            await Assert.ThrowsAsync<CashSchedulerException>(() => 
                TransactionTypeService.Delete(TransactionType.Options.Expense.ToString()));
        }
    }
}
