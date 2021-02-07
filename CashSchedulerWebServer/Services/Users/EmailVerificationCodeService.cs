using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;

namespace CashSchedulerWebServer.Services.Users
{
    public class EmailVerificationCodeService
    {
        private IContextProvider ContextProvider { get; }

        public EmailVerificationCodeService(IContextProvider contextProvider)
        {
            ContextProvider = contextProvider;
        }
        

        public UserEmailVerificationCode GetByUserId(int id)
        {
            return ContextProvider.GetRepository<IUserEmailVerificationCodeRepository>().GetByUserId(id);
        }

        public Task<UserEmailVerificationCode> Create(UserEmailVerificationCode emailVerificationCode)
        {
            return ContextProvider.GetRepository<IUserEmailVerificationCodeRepository>().Create(emailVerificationCode);
        }

        public Task<UserEmailVerificationCode> Update(UserEmailVerificationCode emailVerificationCode)
        {
            var verificationCodeRepository = ContextProvider.GetRepository<IUserEmailVerificationCodeRepository>();
            
            var verificationCode = GetByUserId(emailVerificationCode.User.Id);
            if (verificationCode == null)
            {
                return verificationCodeRepository.Create(emailVerificationCode);
            }

            verificationCode.Code = emailVerificationCode.Code;
            verificationCode.ExpiredDate = emailVerificationCode.ExpiredDate;

            return verificationCodeRepository.Update(verificationCode);
        }
        
        public Task<UserEmailVerificationCode> Delete(int id)
        {
            var verificationCodeRepository = ContextProvider.GetRepository<IUserEmailVerificationCodeRepository>();

            var emailVerificationCode = verificationCodeRepository.GetById(id);
            if (emailVerificationCode == null)
            {
                throw new CashSchedulerException("There is no such verification code");
            }
            
            return verificationCodeRepository.Delete(id);
        }
    }
}
