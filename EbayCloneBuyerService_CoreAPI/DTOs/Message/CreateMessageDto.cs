using System.ComponentModel.DataAnnotations;

namespace EbayCloneBuyerService_CoreAPI.DTOs.Message
{
    public class CreateMessageDto
    {
        [Required]
        public int ReceiverId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }

        [RegularExpression("^(general|order|product_inquiry|complaint)$")]
        public string MessageType { get; set; } = "general";

        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
    }
}
