using CPW_Cityworks.Abstractions;
using CPW_PaymentTransaction.Abstractions;
using CPW_PaymentTransaction.Events;

namespace CityworksOfficeServiceAppTests;

public sealed class PaymentTransactionEventDataBuilder
{
    private static int paymentTransactionID = 1;
    private static int lineItemID = 11;
    private static int appliedPaymentID = 21;

    private readonly PaymentTransactionEventData eventData;

    public PaymentTransactionEventDataBuilder(CaseDetailModel caseDetail)
    {
        eventData = new PaymentTransactionEventData
        {
            ID = paymentTransactionID,
            SourceAppCode = "PLL",
            SourceAppInstanceCode = "Customer",
            SourceKey = caseDetail.Case.ID.ToString(),
        };
        paymentTransactionID++;
    }

    public LineItemEventDataBuilder AddLine(CaseFeeModel caseFee, decimal amountCharged = 0) => new(caseFee, amountCharged, this);

    public PaymentTransactionEventData Build() => eventData;

    public sealed class LineItemEventDataBuilder
    {
        private readonly PaymentTransactionEventDataBuilder builder;
        private readonly LineItemEventData eventData;

        public LineItemEventDataBuilder(CaseFeeModel caseFee, decimal amountCharged, PaymentTransactionEventDataBuilder builder)
        {
            this.builder = builder;
            eventData = new LineItemEventData
            {
                ID = lineItemID,
                SourceKey = caseFee.ID.ToString(),
                AmountCharged = amountCharged == 0 ? caseFee.Amount : amountCharged
            };
            builder.eventData.LineItems = builder.eventData.LineItems.Union([eventData]).ToArray();
            lineItemID++;
        }

        public LineItemEventDataBuilder AddLine(CaseFeeModel caseFee, decimal amountCharged = 0) => builder.AddLine(caseFee, amountCharged);

        public AppliedPaymentEventDataBuilder AddAppliedPayment(decimal amountPaid, PaymentMethod paymentMethod) => 
            new AppliedPaymentEventDataBuilder(amountPaid, paymentMethod, builder, this);

        public PaymentTransactionEventData Build() => builder.Build();

        public sealed class AppliedPaymentEventDataBuilder
        {
            private readonly PaymentTransactionEventDataBuilder builder;
            private readonly LineItemEventDataBuilder lineItemBuilder;
            private readonly AppliedPaymentEventData eventData;

            public AppliedPaymentEventDataBuilder(decimal amountPaid, PaymentMethod paymentMethod, PaymentTransactionEventDataBuilder builder, LineItemEventDataBuilder lineItemBuilder)
            {
                this.builder = builder;
                this.lineItemBuilder = lineItemBuilder;
                eventData = new AppliedPaymentEventData
                {
                    ID = appliedPaymentID,
                    AmountPaid = amountPaid,
                    PaymentMethod = paymentMethod
                };
                lineItemBuilder.eventData.AppliedPayments = lineItemBuilder.eventData.AppliedPayments.Union([eventData]).ToArray();
                appliedPaymentID++;
            }

            public LineItemEventDataBuilder AddLine(CaseFeeModel caseFee, decimal amountCharged = 0) => builder.AddLine(caseFee, amountCharged);

            public AppliedPaymentEventDataBuilder AddAppliedPayment(decimal amountPaid, PaymentMethod paymentMethod) => 
                lineItemBuilder.AddAppliedPayment(amountPaid, paymentMethod);

            public PaymentTransactionEventData Build() => builder.Build();
        }
    }
}