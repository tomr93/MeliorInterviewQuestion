using Melior.InterviewQuestion.Data;
using Melior.InterviewQuestion.Exceptions;
using Melior.InterviewQuestion.Factories;
using Melior.InterviewQuestion.Types;

namespace Melior.InterviewQuestion.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStore _accountDataStore;

        // I could inject IAccountDataStore directly into the constructor and assume that a setup logic would call the factory to create
        // the correct instance of IAccountDataStore, however for completeness I wanted to demonstrate how I could use a factory to create
        // the instance of the IAccountDataStore depending on the configuration settings
        public PaymentService(IAccountDataStoreFactory accountDataStoreFactory)
        {
            _accountDataStore = accountDataStoreFactory.GetAccountDataStore();
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            var result = new MakePaymentResult()
            {
                Success = false
            };

            Account account = _accountDataStore.GetAccount(request.DebtorAccountNumber);

            if (account == null)
            {
                return result;
            }

            var accountValidForPayment = IsAccountValidForPayment(account, request);
            if (!accountValidForPayment)
            {
                return result;
            }

            account.Balance -= request.Amount;
            _accountDataStore.UpdateAccount(account);

            result.Success = true;
            return result;
        }

        private static bool IsAccountValidForPayment(Account account, MakePaymentRequest request)
        {
            switch (request.PaymentScheme)
            {
                case PaymentScheme.Bacs:
                    return account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs);
                case PaymentScheme.FasterPayments:
                    return account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments) && HasSufficientBalance(account.Balance, request.Amount);
                case PaymentScheme.Chaps:
                    return account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps) && account.Status == AccountStatus.Live;
                default:
                    throw new MissingPaymentSchemeException(request.PaymentScheme);
            }
        }

        private static bool HasSufficientBalance(decimal balance, decimal amountToPay)
        {
            return balance >= amountToPay;
        }
    }
}
