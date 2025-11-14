using System.Net;

namespace EbayCloneBuyerService_CoreAPI.Exceptions
{
    public class ServiceException : Exception
    {
        public int StatusCode { get; }

        public ServiceException()
            : base("Đã xảy ra lỗi dịch vụ.") 
        {
            StatusCode = 500; 
        }

        public ServiceException(string message)
            : base(message)
        {
            StatusCode = 500; 
        }

        public ServiceException(string message, int statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public ServiceException(string message, int statusCode, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public ServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = 500; 
        }
        public ServiceException(string message, HttpStatusCode statusCode)
            : base(message)
        {
            StatusCode = (int)statusCode;
        }

        public ServiceException(string message, HttpStatusCode statusCode, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = (int)statusCode;
        }
    }
}
