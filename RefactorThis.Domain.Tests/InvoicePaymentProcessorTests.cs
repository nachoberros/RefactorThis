using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        [TestCase(0, 0, 2, 0, "The invoice is in an invalid state, it has an amount of 0 and it has payments")]
        [TestCase(null, 0, null, 0, "There is no invoice matching this payment")]
        [TestCase(0, 0, null, 0, "No payment needed")]
        [TestCase(10, 10, 10, 0, "Invoice was already fully paid")]
        [TestCase(10, 5, 5, 6, "The payment is greater than the partial amount remaining")]
        [TestCase(5, 0, 0, 6, "The payment is greater than the invoice amount")]
        [TestCase(10, 5, 5, 5, "Final partial payment received, invoice is now fully paid")]
        [TestCase(10, 0, 10, 10, "Invoice was already fully paid")]
        [TestCase(10, 5, 5, 1, "Another partial payment received, still not fully paid")]
        [TestCase(10, 0, 0, 1, "Invoice is now partially paid")]
        public void ProcessPayment_WithPreviousPayments(
          decimal? invoiceAmount,
          decimal amountPaid,
          decimal? previousPaymentAmount,
          decimal newPaymentAmount,
          string expectedMessage)
        {
            var repo = new InvoiceRepository();

            List<Payment> payments = null;
            if (previousPaymentAmount.HasValue)
            {
                payments = previousPaymentAmount > 0 ? new List<Payment> { new Payment { Amount = previousPaymentAmount.Value } } : new List<Payment>();
            }

            if (invoiceAmount.HasValue)
            {
                var invoice = new Invoice(repo)
                {
                    Amount = invoiceAmount.Value,
                    AmountPaid = amountPaid,
                    Payments = payments
                };

                repo.Add(invoice);
            }

            var paymentProcessor = new InvoiceService(repo);
            var payment = newPaymentAmount > 0 ? new Payment { Amount = newPaymentAmount } : new Payment();

            string result = string.Empty;
            try 
            {
                result = paymentProcessor.ProcessPayment(payment);
            }
            catch(Exception ex)
            {
                result = ex.Message;
            }

            Assert.AreEqual(expectedMessage, result);
        }
    }
}
