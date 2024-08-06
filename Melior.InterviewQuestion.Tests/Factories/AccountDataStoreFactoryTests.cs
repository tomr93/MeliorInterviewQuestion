using FluentAssertions;
using Melior.InterviewQuestion.Data;
using Melior.InterviewQuestion.Factories;
using System.Configuration;

namespace Melior.InterviewQuestion.Tests.Factories
{
    [TestClass]
    public class AccountDataStoreFactoryTests
    {
        [TestMethod]
        public void ShouldReturnBackupAccountDataStore_WhenConfigurationAppsettingsDataStoreTypeIsBackup()
        {
            ConfigurationManager.AppSettings["DataStoreType"] = "Backup";

            var sut = new AccountDataStoreFactory();

            var result = sut.GetAccountDataStore();

            result.GetType().Name.Should().Be(nameof(BackupAccountDataStore));
        }

        [TestMethod]
        public void ShouldReturnAccountDataStore_WhenConfigurationhAppsettingsDataStoreTypeIsNotDefined()
        {
            ConfigurationManager.AppSettings["DataStoreType"] = string.Empty;

            var sut = new AccountDataStoreFactory();

            var result = sut.GetAccountDataStore();

            result.GetType().Name.Should().Be(nameof(AccountDataStore));
        }
    }
}
