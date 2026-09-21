namespace AutoUchet.Api.DTOs
{
    public class CreateReceiptRequestDto
    {
        public string BuyerType { get; set; }
        public decimal Amount { get; set; }
        public string PurposeOfPayment { get; set; }

    }
}
