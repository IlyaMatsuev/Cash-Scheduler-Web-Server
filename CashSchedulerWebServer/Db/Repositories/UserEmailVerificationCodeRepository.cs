using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class UserEmailVerificationCodeRepository : IUserEmailVerificationCodeRepository
    {
        private CashSchedulerContext Context { get; }

        public UserEmailVerificationCodeRepository(CashSchedulerContext context)
        {
            Context = context;
        }


        public IEnumerable<UserEmailVerificationCode> GetAll()
        {
            throw new CashSchedulerException("It's forbidden to fetch all the codes");
        }

        public UserEmailVerificationCode GetById(int id)
        {
            return Context.UserEmailVerificationCodes.FirstOrDefault(t => t.Id == id);
        }

        public UserEmailVerificationCode GetByUserId(int id)
        {
            return Context.UserEmailVerificationCodes.FirstOrDefault(t => t.User.Id == id);
        }

        public async Task<UserEmailVerificationCode> Create(UserEmailVerificationCode emailVerificationCode)
        {
            ModelValidator.ValidateModelAttributes(emailVerificationCode);
            Context.UserEmailVerificationCodes.Add(emailVerificationCode);
            await Context.SaveChangesAsync();
            return emailVerificationCode;
        }

        public async Task<UserEmailVerificationCode> Update(UserEmailVerificationCode emailVerificationCode)
        {
            var verificationCode = GetByUserId(emailVerificationCode.User.Id);
            if (verificationCode == null)
            {
                ModelValidator.ValidateModelAttributes(emailVerificationCode);
                Context.UserEmailVerificationCodes.Add(verificationCode = emailVerificationCode);
            }
            else
            {
                verificationCode.Code = emailVerificationCode.Code;
                verificationCode.ExpiredDate = emailVerificationCode.ExpiredDate;
                ModelValidator.ValidateModelAttributes(verificationCode);
                Context.UserEmailVerificationCodes.Update(verificationCode);
            }

            await Context.SaveChangesAsync();

            return verificationCode;
        }
        
        public async Task<UserEmailVerificationCode> Delete(int emailVerificationCodeId)
        {
            var emailVerificationCode = GetById(emailVerificationCodeId);
            if (emailVerificationCode == null)
            {
                throw new CashSchedulerException("There is no such verification code");
            }
            Context.UserEmailVerificationCodes.Remove(emailVerificationCode);
            await Context.SaveChangesAsync();
            return emailVerificationCode;
        }
    }
}
