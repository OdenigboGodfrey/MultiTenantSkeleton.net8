using webapi_80.src.User.Models;

namespace webapi_80.src.Shared.ViewModels
{
    public class ApiResponse<T>
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public T Data { get; set; }
        public UserModel user { get; internal set; }
    }
}
