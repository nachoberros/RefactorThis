using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        private readonly InvoiceRepository _invoiceRepository;

        public InvoiceService(InvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        #region Public Methods

        public string ProcessPayment(Payment payment)
        {
            var inv = _invoiceRepository.GetInvoice(payment.Reference) ?? throw new InvalidOperationException("There is no invoice matching this payment");

            var responseMessage = string.Empty;

            if (inv.Amount == 0)
            {
                if (inv.Payments == null || !inv.Payments.Any())
                {
                    responseMessage = "No payment needed";
                    return responseMessage;
                }
                else
                {
                    throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments");
                }
            }

            if (payment.Amount > inv.Amount)
            {
                responseMessage = "The payment is greater than the invoice amount";
                return responseMessage;
            }

            if (inv.Payments != null && inv.Payments.Any())
            {
                var totalPaid = inv.Payments.Sum(x => x.Amount);
                if (totalPaid != 0 && inv.Amount == totalPaid)
                {
                    responseMessage = "Invoice was already fully paid";
                    return responseMessage;
                }
                else if (totalPaid != 0 && payment.Amount > (inv.Amount - inv.AmountPaid))
                {
                    responseMessage = "The payment is greater than the partial amount remaining";
                    return responseMessage;
                }
                else
                {
                    bool isFullyPaid = (inv.Amount - inv.AmountPaid) == payment.Amount;
                    AddPaymentToInvoice(payment, inv);
                    responseMessage = isFullyPaid ? "Final partial payment received, invoice is now fully paid" : "Another partial payment received, still not fully paid";
                }
            }
            else
            {
                bool isFullyPaid = inv.Amount == payment.Amount;
                AddPaymentToInvoice(payment, inv);
                responseMessage = isFullyPaid ? "Invoice is now fully paid" : "Invoice is now partially paid";
            }

            inv.Save();

            return responseMessage;
        }

        #endregion

        #region Private Methods

        private static void AddPaymentToInvoice(Payment payment, Invoice inv)
        {
            if (inv.Type == InvoiceType.Commercial)
            {
                inv.TaxAmount += payment.Amount * 0.14m;
            }

            inv.AmountPaid += payment.Amount;
            inv.Payments.Add(payment);
        }

        #endregion
    }
}