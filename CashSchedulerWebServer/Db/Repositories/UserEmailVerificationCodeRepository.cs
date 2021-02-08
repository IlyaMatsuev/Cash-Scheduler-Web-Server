﻿using CashSchedulerWebServer.Db.Contracts;
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


        public UserEmailVerificationCode GetById(int id)
        {
            return Context.UserEmailVerificationCodes.FirstOrDefault(t => t.Id == id);
        }

        public UserEmailVerificationCode GetByUserId(int id)
        {
            return Context.UserEmailVerificationCodes.FirstOrDefault(t => t.User.Id == id);
        }
        
        public IEnumerable<UserEmailVerificationCode> GetAll()
        {
            throw new CashSchedulerException("It's forbidden to fetch all the codes");
        }

        public async Task<UserEmailVerificationCode> Create(UserEmailVerificationCode emailVerificationCode)
        {
            ModelValidator.ValidateModelAttributes(emailVerificationCode);
            
            await Context.UserEmailVerificationCodes.AddAsync(emailVerificationCode);
            await Context.SaveChangesAsync();
            
            return emailVerificationCode;
        }

        public async Task<UserEmailVerificationCode> Update(UserEmailVerificationCode verificationCode)
        {
            ModelValidator.ValidateModelAttributes(verificationCode);
            
            Context.UserEmailVerificationCodes.Update(verificationCode);
            await Context.SaveChangesAsync();

            return verificationCode;
        }
        
        public async Task<UserEmailVerificationCode> Delete(int emailVerificationCodeId)
        {
            var emailVerificationCode = GetById(emailVerificationCodeId);
            
            Context.UserEmailVerificationCodes.Remove(emailVerificationCode);
            await Context.SaveChangesAsync();
            
            return emailVerificationCode;
        }
    }
}
