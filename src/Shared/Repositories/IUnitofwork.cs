using webapi_80.src.Tenant.Contract;
using webapi_80.src.User.Contract;
// using webapi_80.src.Weather.Contracts;

namespace webapi_80.src.Shared.Contract
{
    public interface IUnitofwork
    {
        // public IWeatherInterface WeatherService { get; }
        public IUserServices UserServices { get; }
        public ITenantService TenantServices { get; }
        string subdomain { get; }
    }
}