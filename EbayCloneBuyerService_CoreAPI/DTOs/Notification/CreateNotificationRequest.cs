namespace EbayCloneBuyerService_CoreAPI.DTOs.Notification
{
    public class CreateNotificationRequest
    {
        public int UserId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int? ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
}
