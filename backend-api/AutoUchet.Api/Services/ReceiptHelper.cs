namespace AutoUchet.Api.Services
{
    public static class ReceiptHelper
    {
        public static string GetActualStatus(string dbStatus, DateTime createdAt)
        {
            if (dbStatus == "WaitingPayment" && DateTime.UtcNow > createdAt.AddHours(24))
            {
                return "Expired";
            }
            return dbStatus;
        }
    }
}