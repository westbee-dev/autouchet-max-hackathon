using System.Text.Json.Serialization;

using System.Text.Json.Serialization;

namespace AutoUchet.Api.Entities
{
    public class Activity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;

        [JsonIgnore]
        public User User { get; set; }
    }
}