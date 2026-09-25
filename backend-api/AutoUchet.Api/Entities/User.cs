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

        [JsonIgnore]
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
        [JsonIgnore]
        public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
    }
}
