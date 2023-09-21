

using webapi_80.src.Shared.DatabaseContext;
using webapi_80.src.Tenant.SchemaTenant;
using webapi_80.src.User.Contract;
using webapi_80.src.User.Services;
// using webapi_80.src.Weather.Contracts;
// using webapi_80.src.Weather.Repositories.Weather;
// using webapi_80.src.Weather.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using webapi_80.src.Tenant.SchemaTenant.SchemaContext;

namespace webapi_80.src.Shared.Contract
{
    public class Unitofwork : IUnitofwork
    {
        // IWeatherRepository weatherRepository;

        private readonly ApplicationDbContext subdomainSchemaContext;
        private readonly ApplicationDbContext publicSchemaContext;
        private IHttpContextAccessor _httpContextAccessor;
        private ITenantSchema tenantSchema;
        
        public Unitofwork(IHttpContextAccessor httpContextAccessor, ITenantSchema tenantSchema) {
            // this.weatherRepository = weatherRepository;
            var _host = tenantSchema.ExtractSubdomainFromRequest(httpContextAccessor.HttpContext);
            this.subdomainSchemaContext = tenantSchema.getRequestContext(_host);
            _httpContextAccessor = httpContextAccessor;
            publicSchemaContext = new ApplicationDbContext(new DbContextSchema());
            this.tenantSchema = tenantSchema;
        }
        
        // public IWeatherInterface GetWeatherService;
        public IUserServices GetUserServices;


        // public IWeatherInterface WeatherService
        // {
        //     get
        //     {
        //         if (this.GetWeatherService == null) { this.GetWeatherService = new WeatherService(weatherRepository); }
        //         return this.GetWeatherService;
        //     }
        // }

        public IUserServices UserServices {
            get
            {
                if (this.GetUserServices == null) {
                    this.GetUserServices = new UserServices(this.publicSchemaContext, this._httpContextAccessor, this.subdomainSchemaContext);
                }
                return this.GetUserServices;
            }
        }
    }
}