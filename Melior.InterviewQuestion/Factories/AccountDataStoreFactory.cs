using Melior.InterviewQuestion.Data;
using System.Configuration;

namespace Melior.InterviewQuestion.Factories
{
    public class AccountDataStoreFactory : IAccountDataStoreFactory
    {
        public IAccountDataStore GetAccountDataStore()
        {
            var dataStoreType = ConfigurationManager.AppSettings["DataStoreType"];

            return dataStoreType == "Backup" ? new BackupAccountDataStore() : new AccountDataStore();
        }
    }
}
