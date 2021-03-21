using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Categories;
using CashSchedulerWebServer.Services.Contracts;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CashSchedulerWebServer.Tests.Services
{
    public class CategoryServiceTest
    {
        private ICategoryService CategoryService { get; }
        private Mock<IWalletService> WalletService { get; }
        private Mock<IEventManager> EventManager { get; }
        private Mock<ITransactionTypeRepository> TransactionTypeRepository { get; }
        private Mock<ICategoryRepository> CategoryRepository { get; }
        private Mock<ITransactionRepository> TransactionRepository { get; }
        private Mock<IRegularTransactionRepository> RegularTransactionRepository { get; }
        private Mock<IUserRepository> UserRepository { get; }
        private Mock<IContextProvider> ContextProvider { get; }
        private Mock<IUserContext> UserContext { get; }

        public CategoryServiceTest()
        {
            ContextProvider = new Mock<IContextProvider>();
            EventManager = new Mock<IEventManager>();
            TransactionTypeRepository = new Mock<ITransactionTypeRepository>();
            CategoryRepository = new Mock<ICategoryRepository>();
            TransactionRepository = new Mock<ITransactionRepository>();
            RegularTransactionRepository = new Mock<IRegularTransactionRepository>();
            UserRepository = new Mock<IUserRepository>();
            WalletService = new Mock<IWalletService>();

            UserContext = new Mock<IUserContext>();

            ContextProvider
                .Setup(c => c.GetRepository<ITransactionTypeRepository>())
                .Returns(TransactionTypeRepository.Object);

            ContextProvider
                .Setup(c => c.GetRepository<ICategoryRepository>())
                .Returns(CategoryRepository.Object);

            ContextProvider
                .Setup(c => c.GetRepository<ITransactionRepository>())
                .Returns(TransactionRepository.Object);

            ContextProvider
                .Setup(c => c.GetRepository<IRegularTransactionRepository>())
                .Returns(RegularTransactionRepository.Object);

            ContextProvider
                .Setup(c => c.GetRepository<IUserRepository>())
                .Returns(UserRepository.Object);

            ContextProvider
                .Setup(c => c.GetService<IWalletService>())
                .Returns(WalletService.Object);

            CategoryService = new CategoryService(ContextProvider.Object, UserContext.Object, EventManager.Object);
        }


        [Fact]
        public void GetALl_ReturnsAllAndByTransactionType()
        {
            const int testingUserId = 1;

            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var categories = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .Where(c => (c.User != null && c.User.Id == testingUserId && c.IsCustom) || !c.IsCustom);

            var expectedIncomeCategories = categories
                .Where(c => c.Type.Name == TransactionType.Options.Income.ToString());

            var expectedExpenseCategories = categories
                .Where(c => c.Type.Name == TransactionType.Options.Expense.ToString());

            CategoryRepository
                .Setup(c => c.GetAll())
                .Returns(categories);

            CategoryRepository
                .Setup(c => c.GetAll(TransactionType.Options.Income.ToString()))
                .Returns(expectedIncomeCategories);

            CategoryRepository
                .Setup(c => c.GetAll(TransactionType.Options.Expense.ToString()))
                .Returns(expectedExpenseCategories);


            var actualCategories = CategoryService.GetAll();
            var actualIncomeCategories = CategoryService.GetAll(TransactionType.Options.Income.ToString());
            var actualExpenseCategories = CategoryService.GetAll(TransactionType.Options.Expense.ToString());


            Assert.NotNull(actualCategories);
            Assert.NotNull(actualIncomeCategories);
            Assert.NotNull(actualExpenseCategories);
            Assert.Equal(categories.Count(), actualCategories.Count());
            Assert.Equal(expectedIncomeCategories.Count(), actualIncomeCategories.Count());
            Assert.Equal(expectedExpenseCategories.Count(), actualExpenseCategories.Count());
        }

        [Fact]
        public void GetStandardCategories_ReturnsAllAndByTransactionType()
        {
            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var categories = JsonConvert.DeserializeObject<List<Category>>(categoriesJson).Where(c => !c.IsCustom);

            var expectedIncomeCategories = categories
                .Where(c => c.Type.Name == TransactionType.Options.Income.ToString());

            var expectedExpenseCategories = categories
                .Where(c => c.Type.Name == TransactionType.Options.Expense.ToString());

            CategoryRepository
                .Setup(c => c.GetStandardCategories(null))
                .Returns(categories);

            CategoryRepository
                .Setup(c => c.GetStandardCategories(TransactionType.Options.Income.ToString()))
                .Returns(expectedIncomeCategories);

            CategoryRepository
                .Setup(c => c.GetStandardCategories(TransactionType.Options.Expense.ToString()))
                .Returns(expectedExpenseCategories);


            var actualCategories = CategoryService.GetStandardCategories();
            var actualIncomeCategories =
                CategoryService.GetStandardCategories(TransactionType.Options.Income.ToString());
            var actualExpenseCategories =
                CategoryService.GetStandardCategories(TransactionType.Options.Expense.ToString());


            Assert.NotNull(actualCategories);
            Assert.NotNull(actualIncomeCategories);
            Assert.NotNull(actualExpenseCategories);
            Assert.Equal(categories.Count(), actualCategories.Count());
            Assert.Equal(expectedIncomeCategories.Count(), actualIncomeCategories.Count());
            Assert.Equal(expectedExpenseCategories.Count(), actualExpenseCategories.Count());
        }

        [Fact]
        public void GetCustomCategories_ReturnsAllAndByTransactionType()
        {
            const int testingUserId = 1;

            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var categories = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .Where(c => c.IsCustom && c.User.Id == testingUserId);

            var expectedIncomeCategories = categories
                .Where(c => c.Type.Name == TransactionType.Options.Income.ToString());

            var expectedExpenseCategories = categories
                .Where(c => c.Type.Name == TransactionType.Options.Expense.ToString());

            CategoryRepository
                .Setup(c => c.GetCustomCategories(null))
                .Returns(categories);

            CategoryRepository
                .Setup(c => c.GetCustomCategories(TransactionType.Options.Income.ToString()))
                .Returns(expectedIncomeCategories);

            CategoryRepository
                .Setup(c => c.GetCustomCategories(TransactionType.Options.Expense.ToString()))
                .Returns(expectedExpenseCategories);


            var actualCategories = CategoryService.GetCustomCategories();
            var actualIncomeCategories = CategoryService.GetCustomCategories(TransactionType.Options.Income.ToString());
            var actualExpenseCategories =
                CategoryService.GetCustomCategories(TransactionType.Options.Expense.ToString());


            Assert.NotNull(actualCategories);
            Assert.NotNull(actualIncomeCategories);
            Assert.NotNull(actualExpenseCategories);
            Assert.Equal(categories.Count(), actualCategories.Count());
            Assert.Equal(expectedIncomeCategories.Count(), actualIncomeCategories.Count());
            Assert.Equal(expectedExpenseCategories.Count(), actualExpenseCategories.Count());
        }

        [Fact]
        public async Task Create_ReturnsNewCategory()
        {
            const int testingUserId = 1;

            string transactionTypesJson =
                File.ReadAllText(TestConfiguration.MockDataFolderPath + @"TransactionTypes.json");
            var transactionTypes = JsonConvert.DeserializeObject<List<TransactionType>>(transactionTypesJson);

            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == testingUserId);

            var category = new Category
            {
                Name = "Test category",
                TypeName = TransactionType.Options.Income.ToString(),
                IsCustom = true
            };

            UserContext.Setup(c => c.GetUserId()).Returns(testingUserId);

            UserRepository.Setup(u => u.GetByKey(testingUserId)).Returns(user);

            TransactionTypeRepository
                .Setup(t => t.GetByKey(TransactionType.Options.Income.ToString()))
                .Returns(transactionTypes.First(t => t.Name == TransactionType.Options.Income.ToString()));

            TransactionTypeRepository
                .Setup(t => t.GetByKey(TransactionType.Options.Expense.ToString()))
                .Returns(transactionTypes.First(t => t.Name == TransactionType.Options.Expense.ToString()));

            CategoryRepository
                .Setup(c => c.Create(category))
                .ReturnsAsync(category);

            var categoryService = new CategoryService(ContextProvider.Object, UserContext.Object, EventManager.Object);


            var resultCategory = await categoryService.Create(category);

            Assert.NotNull(resultCategory);
            Assert.NotNull(resultCategory.Type);
            Assert.NotNull(resultCategory.User);
            Assert.Equal(category.Name, resultCategory.Name);
            Assert.Equal(category.IsCustom, resultCategory.IsCustom);
            Assert.Equal(category.Type.Name, resultCategory.Type.Name);
            Assert.Equal(category.User.Id, resultCategory.User.Id);
            Assert.Equal("/static/icons/categories/unknown.png", resultCategory.IconUrl);
        }

        [Fact]
        public async Task Update_ReturnsUpdatedCategory()
        {
            const int testingUserId = 1;

            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var categories = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .Where(c => !c.IsCustom || c.User.Id == testingUserId);

            var oldCategory = categories.First();

            var category = new Category
            {
                Id = oldCategory.Id,
                Name = "New name for test",
                IconUrl = "/static/icons/categories/update-category.png"
            };

            CategoryRepository
                .Setup(c => c.GetByKey(category.Id))
                .Returns(oldCategory);

            CategoryRepository
                .Setup(c => c.Update(oldCategory))
                .ReturnsAsync(oldCategory);


            var resultCategory = await CategoryService.Update(category);


            Assert.NotNull(resultCategory);
            Assert.Equal(oldCategory.Name, resultCategory.Name);
            Assert.Equal(oldCategory.IconUrl, resultCategory.IconUrl);
            Assert.Equal(oldCategory.IsCustom, resultCategory.IsCustom);
            Assert.Equal(oldCategory.Type.Name, resultCategory.Type.Name);

            if (resultCategory.IsCustom)
            {
                Assert.Equal(oldCategory.User.Id, resultCategory.User.Id);
            }
        }

        [Fact]
        public async Task Delete_ReturnsDeletedCategory()
        {
            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var categories = JsonConvert.DeserializeObject<List<Category>>(categoriesJson).Where(c => c.IsCustom);

            var oldCategory = categories.First();

            int categoryToRemoveId = oldCategory.Id;

            CategoryRepository
                .Setup(c => c.GetByKey(categoryToRemoveId))
                .Returns(oldCategory);

            CategoryRepository
                .Setup(c => c.Delete(categoryToRemoveId))
                .ReturnsAsync(oldCategory);


            // NOTE: balance changing is not testing in this scope
            var resultCategory = await CategoryService.Delete(categoryToRemoveId);


            Assert.NotNull(resultCategory);
            Assert.Equal(oldCategory.Name, resultCategory.Name);
            Assert.Equal(oldCategory.IconUrl, resultCategory.IconUrl);
            Assert.Equal(oldCategory.IsCustom, resultCategory.IsCustom);
            Assert.Equal(oldCategory.Type.Name, resultCategory.Type.Name);
            Assert.Equal(oldCategory.User.Id, resultCategory.User.Id);
        }

        [Fact]
        public async Task Delete_ThrowsException()
        {
            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var categories = JsonConvert.DeserializeObject<List<Category>>(categoriesJson);

            var standardCategory = categories.First(c => !c.IsCustom);
            int randomCategoryId = 11;

            CategoryRepository
                .Setup(c => c.GetByKey(standardCategory.Id))
                .Returns(standardCategory);

            CategoryRepository
                .Setup(c => c.GetByKey(randomCategoryId))
                .Returns((Category) null);

            CategoryRepository
                .Setup(c => c.Delete(standardCategory.Id))
                .ReturnsAsync(standardCategory);


            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await CategoryService.Delete(randomCategoryId);
            });

            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await CategoryService.Delete(standardCategory.Id);
            });
        }
    }
}
