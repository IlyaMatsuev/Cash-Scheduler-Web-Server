using System;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.WebServices.Salesforce.SObjects
{
    public class SfContact : SfObject
    {
        public override string SObjectTypeName => "Contact";

        public string FirstName { get; }

        public string LastName { get; }

        public string Email { get; }
        
        public DateTime? LastLoginDate__c { get; }

        public SfContact(int id) : base(id) {}

        public SfContact(User user)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName ?? user.Email;
            Email = user.Email;
            LastLoginDate__c = user.LastLoginDate;
        }

        public SfContact(User user, int id) : base(id)
        {
            FirstName = user.FirstName;
            LastName = user.LastName ?? user.Email;
            Email = user.Email;
            LastLoginDate__c = user.LastLoginDate;
        }
    }
}
