using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class LanguageRepository : ILanguageRepository
    {
        private CashSchedulerContext Context { get; }

        public LanguageRepository(CashSchedulerContext context)
        {
            Context = context;
        }


        public IEnumerable<Language> GetAll()
        {
            return Context.Languages;
        }

        public Language GetByKey(string abbreviation)
        {
            return Context.Languages.FirstOrDefault(l => l.Abbreviation == abbreviation);
        }

        public Task<Language> Create(Language language)
        {
            throw new CashSchedulerException("It's forbidden to create new languages");
        }

        public Task<Language> Update(Language language)
        {
            throw new CashSchedulerException("It's forbidden to update existing languages");
        }

        public Task<Language> Delete(string abbreviation)
        {
            throw new CashSchedulerException("It's forbidden to delete existing languages");
        }
    }
}
