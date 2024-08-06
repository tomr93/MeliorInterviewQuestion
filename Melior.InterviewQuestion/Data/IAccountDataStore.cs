using Melior.InterviewQuestion.Types;

namespace Melior.InterviewQuestion.Data
{
    public interface IAccountDataStore
    {
        public Account GetAccount(string accountNumber);

        public void UpdateAccount(Account account);
    }
}
