using Melior.InterviewQuestion.Data;

namespace Melior.InterviewQuestion.Factories
{
    public interface IAccountDataStoreFactory
    {
        public IAccountDataStore GetAccountDataStore();
    }
}
