using Melior.InterviewQuestion.Types;
using System;

namespace Melior.InterviewQuestion.Exceptions
{
    public class MissingPaymentSchemeException : Exception
    {
        public MissingPaymentSchemeException(PaymentScheme paymentScheme) : base($"Missing PaymentScheme for {paymentScheme}") { }
    }
}
