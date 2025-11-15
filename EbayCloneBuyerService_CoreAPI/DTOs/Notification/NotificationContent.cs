using System.Text.Json.Serialization;

namespace EbayCloneBuyerService_CoreAPI.DTOs.Notification
{
    public class NotificationContent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } // ORDER_UPDATE, PROMOTION, ADMIN_FEEDBACK, REVIEW_REPLY

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("referenceId")]
        public int? ReferenceId { get; set; } // Order ID, Product ID, Review ID

        [JsonPropertyName("referenceType")]
        public string ReferenceType { get; set; } // ORDER, PRODUCT, REVIEW

        [JsonPropertyName("isRead")]
        public bool IsRead { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; set; } // Extra data
    }

}
