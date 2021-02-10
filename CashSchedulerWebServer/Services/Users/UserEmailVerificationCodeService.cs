using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;

namespace CashSchedulerWebServer.Services.Users
{
    public class UserEmailVerificationCodeService : IUserEmailVerificationCodeService
    {
        private IUserEmailVerificationCodeRepository EmailVerificationCodeRepository { get; }

        public UserEmailVerificationCodeService(IContextProvider contextProvider)
        {
            EmailVerificationCodeRepository = contextProvider.GetRepository<IUserEmailVerificationCodeRepository>();
        }
        

        public UserEmailVerificationCode GetByUserId(int id)
        {
            return EmailVerificationCodeRepository.GetByUserId(id);
        }

        public Task<UserEmailVerificationCode> Create(UserEmailVerificationCode emailVerificationCode)
        {
            return EmailVerificationCodeRepository.Create(emailVerificationCode);
        }

        public Task<UserEmailVerificationCode> Update(UserEmailVerificationCode emailVerificationCode)
        {
            var verificationCode = GetByUserId(emailVerificationCode.User.Id);
            if (verificationCode == null)
            {
                return EmailVerificationCodeRepository.Create(emailVerificationCode);
            }

            verificationCode.Code = emailVerificationCode.Code;
            verificationCode.ExpiredDate = emailVerificationCode.ExpiredDate;

            return EmailVerificationCodeRepository.Update(verificationCode);
        }
        
        public Task<UserEmailVerificationCode> Delete(int id)
        {
            var emailVerificationCode = EmailVerificationCodeRepository.GetByKey(id);
            if (emailVerificationCode == null)
            {
                throw new CashSchedulerException("There is no such verification code");
            }
            
            return EmailVerificationCodeRepository.Delete(id);
        }
    }
}
