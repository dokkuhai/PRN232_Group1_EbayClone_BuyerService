namespace EbayCloneBuyerService_CoreAPI.DTOs.Review
{
    public class ReviewResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductTitle { get; set; } // Join từ Product
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; } // Join từ User
        public string ReviewerAvatar { get; set; } // Join từ User
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsOwner { get; set; } // Check xem user hiện tại có phải owner không
    }
}




