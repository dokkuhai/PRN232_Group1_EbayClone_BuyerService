namespace EbayCloneBuyerService_CoreAPI.Services.Interface
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string body);
    }
}
