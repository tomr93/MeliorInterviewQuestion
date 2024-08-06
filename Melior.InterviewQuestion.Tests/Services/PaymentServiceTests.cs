using AutoFixture;
using FluentAssertions;
using Melior.InterviewQuestion.Data;
using Melior.InterviewQuestion.Factories;
using Melior.InterviewQuestion.Services;
using Melior.InterviewQuestion.Types;
using Moq;

namespace Melior.InterviewQuestion.Tests.Services
{
    [TestClass]
    public class PaymentServiceTests
    {
        private static Fixture Fixture => new();

        [TestMethod]
        public void AccountNotFound_ResultShouldBeFalse()
        {
            var mockAccountDataStore = new Mock<IAccountDataStore>();

            mockAccountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns((Account)null);

            var mockDataStoreFactory = SetupMockDataStoreFactory(mockAccountDataStore.Object);

            var sut = new PaymentService(mockDataStoreFactory.Object);

            var result = sut.MakePayment(Fixture.Build<MakePaymentRequest>().Create());

            result.Success.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(PaymentScheme.Bacs)]
        [DataRow(PaymentScheme.FasterPayments)]
        [DataRow(PaymentScheme.Chaps)]
        public void AllowedPaymentSchemes_InvalidFlag_ShouldReturnFalseAndNotUpdatedAccount(PaymentScheme paymentScheme)
        {
            var mockAccountDataStore = new Mock<IAccountDataStore>();

            const decimal balance = 1000;
            const decimal paymentAmount = balance - 1;

            var account = new Account()
            {
                AccountNumber = Fixture.Create<string>(),
                Balance = balance,
                Status = AccountStatus.Live
            };

            mockAccountDataStore.Setup(x => x.GetAccount(account.AccountNumber)).Returns(account);

            var mockDataStoreFactory = SetupMockDataStoreFactory(mockAccountDataStore.Object);

            var sut = new PaymentService(mockDataStoreFactory.Object);

            var paymentRequest = GetMakePaymentRequest(paymentAmount, paymentScheme, account.AccountNumber);

            var result = sut.MakePayment(paymentRequest);

            result.Success.Should().BeFalse();

            mockAccountDataStore.Verify(x => x.UpdateAccount(It.IsAny<Account>()), Times.Never());
        }

        [TestMethod]
        public void Bacs_ValidAccount_ResultShouldBeTrue_AccountShouldBeUPdatedWithNewBalance()
        {
            var mockAccountDataStore = new Mock<IAccountDataStore>();

            const decimal balance = 1000;
            const decimal paymentAmount = balance - 1;

            var account = new Account()
            {
                AccountNumber = Fixture.Create<string>(),
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs,
                Balance = balance,
                Status = AccountStatus.Live
            };

            mockAccountDataStore.Setup(x => x.GetAccount(account.AccountNumber)).Returns(account);

            var mockDataStoreFactory = SetupMockDataStoreFactory(mockAccountDataStore.Object);

            var sut = new PaymentService(mockDataStoreFactory.Object);

            var paymentRequest = GetMakePaymentRequest(paymentAmount, PaymentScheme.Bacs, account.AccountNumber);

            var result = sut.MakePayment(paymentRequest);

            result.Success.Should().BeTrue();

            VerifyUpdateAccountMethodOnAccountDataStore(mockAccountDataStore, account, balance, paymentAmount);
        }

        [TestMethod]
        public void FasterPayments_ValidAccount_ResultShouldBeTrue_AccountShouldBeUpdatedWithNewBalance()
        {
            var mockAccountDataStore = new Mock<IAccountDataStore>();

            const decimal balance = 1000;
            const decimal paymentAmount = balance - 1;

            var validAccount = new Account()
            {
                AccountNumber = Fixture.Create<string>(),
                AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments,
                Balance = balance,
                Status = AccountStatus.Live
            };

            mockAccountDataStore.Setup(x => x.GetAccount(validAccount.AccountNumber)).Returns(validAccount);

            var mockDataStoreFactory = SetupMockDataStoreFactory(mockAccountDataStore.Object);

            var sut = new PaymentService(mockDataStoreFactory.Object);

            var paymentRequest = GetMakePaymentRequest(paymentAmount, PaymentScheme.FasterPayments, validAccount.AccountNumber);

            var result = sut.MakePayment(paymentRequest);

            result.Success.Should().BeTrue();

            VerifyUpdateAccountMethodOnAccountDataStore(mockAccountDataStore, validAccount, balance, paymentAmount);
        }

        [TestMethod]
        public void FasterPayments_BalanceLessThanAmount_ResultShouldBeFalse_AccountShouldNotBeUpdated()
        {
            var mockAccountDataStore = new Mock<IAccountDataStore>();

            const decimal balance = 1000;
            const decimal paymentAmount = balance + 1;

            var account = new Account()
            {
                AccountNumber = Fixture.Create<string>(),
                AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments,
                Balance = balance,
                Status = AccountStatus.Live
            };

            mockAccountDataStore.Setup(x => x.GetAccount(account.AccountNumber)).Returns(account);

            var mockDataStoreFactory = SetupMockDataStoreFactory(mockAccountDataStore.Object);

            var sut = new PaymentService(mockDataStoreFactory.Object);

            var paymentRequest = GetMakePaymentRequest(paymentAmount, PaymentScheme.FasterPayments, account.AccountNumber);

            var result = sut.MakePayment(paymentRequest);

            result.Success.Should().BeFalse();

            mockAccountDataStore.Verify(x => x.UpdateAccount(It.IsAny<Account>()), Times.Never());
        }

        [TestMethod]
        public void Chaps_ValidAccount_ResultShouldBeTrue_AccountShouldBeUPdatedWithNewBalance()
        {
            var mockAccountDataStore = new Mock<IAccountDataStore>();

            const decimal balance = 1000;
            const decimal paymentAmount = balance - 1;

            var account = new Account()
            {
                AccountNumber = Fixture.Create<string>(),
                AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps,
                Balance = balance,
                Status = AccountStatus.Live
            };

            mockAccountDataStore.Setup(x => x.GetAccount(account.AccountNumber)).Returns(account);

            var mockDataStoreFactory = SetupMockDataStoreFactory(mockAccountDataStore.Object);

            var sut = new PaymentService(mockDataStoreFactory.Object);

            var paymentRequest = GetMakePaymentRequest(paymentAmount, PaymentScheme.Chaps, account.AccountNumber);

            var result = sut.MakePayment(paymentRequest);

            result.Success.Should().BeTrue();

            VerifyUpdateAccountMethodOnAccountDataStore(mockAccountDataStore, account, balance, paymentAmount);
        }

        [TestMethod]
        [DataRow(AccountStatus.Disabled)]
        [DataRow(AccountStatus.InboundPaymentsOnly)]
        public void Chaps_AccountStatusNotLive_ResultShouldBeFalse_AccountShouldNotBeUpdated(AccountStatus accountStatus)
        {
            var mockAccountDataStore = new Mock<IAccountDataStore>();

            const decimal balance = 1000;
            const decimal paymentAmount = balance - 1;

            var account = new Account()
            {
                AccountNumber = Fixture.Create<string>(),
                AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps,
                Balance = balance,
                Status = accountStatus
            };

            mockAccountDataStore.Setup(x => x.GetAccount(account.AccountNumber)).Returns(account);

            var mockDataStoreFactory = SetupMockDataStoreFactory(mockAccountDataStore.Object);

            var sut = new PaymentService(mockDataStoreFactory.Object);

            var paymentRequest = GetMakePaymentRequest(paymentAmount, PaymentScheme.Chaps, account.AccountNumber);

            var result = sut.MakePayment(paymentRequest);

            result.Success.Should().BeFalse();

            mockAccountDataStore.Verify(x => x.UpdateAccount(It.IsAny<Account>()), Times.Never());
        }

        #region helper methods
        private static Mock<IAccountDataStoreFactory> SetupMockDataStoreFactory(IAccountDataStore accountDataStoreToReturn)
        {
            var mockDataStoreFactory = new Mock<IAccountDataStoreFactory>();
            mockDataStoreFactory.Setup(x => x.GetAccountDataStore()).Returns(accountDataStoreToReturn);
            return mockDataStoreFactory;
        }

        private MakePaymentRequest GetMakePaymentRequest(decimal paymentAmount, PaymentScheme paymentScheme, string debtorAccountNumber) =>
            Fixture.Build<MakePaymentRequest>()
                .With(x => x.DebtorAccountNumber, debtorAccountNumber)
                .With(x => x.Amount, paymentAmount)
                .With(x => x.PaymentScheme, paymentScheme)
                .Create();

        private void VerifyUpdateAccountMethodOnAccountDataStore(Mock<IAccountDataStore> mockAccountDataStore, Account validAccount, decimal balance, decimal paymentAmount)
        {
            mockAccountDataStore.Verify(x => x.UpdateAccount(It.Is<Account>(
                    x =>
                        x.AccountNumber == validAccount.AccountNumber &&
                        x.AllowedPaymentSchemes == validAccount.AllowedPaymentSchemes &&
                        x.Balance == balance - paymentAmount &&
                        x.Status == validAccount.Status
                    )));
        }
        #endregion
    }

}
