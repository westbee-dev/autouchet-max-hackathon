using System.Text.Json.Serialization;

namespace AutoUchet.Api.Entities
{
    public class User
    {
        public int Id { get; set; }
        public long MaxUserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public bool RemindAboutTax { get; set; } = true;
        public decimal TaxRate { get; set; } = 0.04m;
        public string ActivityType { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
    }
}
