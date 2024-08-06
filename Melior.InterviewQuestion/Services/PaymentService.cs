using Melior.InterviewQuestion.Data;
using Melior.InterviewQuestion.Factories;
using Melior.InterviewQuestion.Types;
using System.Configuration;

namespace Melior.InterviewQuestion.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStore _accountDataStore;

        // Could inject IAccountDataStore directly into the constructor and assume that a setup logic would call the factory to create
        // the correct instance of IAccountDataStore, however for completeness I wanted to demonstrate how we could use a factory to create the instance
        // of the accountDataStore depending on the configuration settings
        public PaymentService(IAccountDataStoreFactory accountDataStoreFactory)
        {
            _accountDataStore = accountDataStoreFactory.GetAccountDataStore();
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            var result = new MakePaymentResult();
            result.Success = true;

            //var dataStoreType = ConfigurationManager.AppSettings["DataStoreType"];

            Account account = _accountDataStore.GetAccount(request.DebtorAccountNumber);

            //if (dataStoreType == "Backup")
            //{
            //    var accountDataStore = new BackupAccountDataStore();
            //    account = accountDataStore.GetAccount(request.DebtorAccountNumber);
            //}
            //else
            //{
            //    var accountDataStore = new AccountDataStore();
            //    account = accountDataStore.GetAccount(request.DebtorAccountNumber);
            //}

            if (account == null)
            {
                result.Success = false;
                return result;
            }

            

            switch (request.PaymentScheme)
            {
                case PaymentScheme.Bacs:
                    if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs))
                    {
                        result.Success = false;
                    }
                    break;

                case PaymentScheme.FasterPayments:
                    if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments))
                    {
                        result.Success = false;
                    }
                    else if (account.Balance < request.Amount)
                    {
                        result.Success = false;
                    }
                    break;

                case PaymentScheme.Chaps:
                    if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps))
                    {
                        result.Success = false;
                    }
                    else if (account.Status != AccountStatus.Live)
                    {
                        result.Success = false;
                    }
                    break;
            }

            if (result.Success)
            {
                account.Balance -= request.Amount;

                _accountDataStore.UpdateAccount(account);

                //if (dataStoreType == "Backup")
                //{
                //    var accountDataStore = new BackupAccountDataStore();
                //    accountDataStore.UpdateAccount(account);
                //}
                //else
                //{
                //    var accountDataStore = new AccountDataStore();
                //    accountDataStore.UpdateAccount(account);
                //}
            }

            return result;
        }
    }
}
