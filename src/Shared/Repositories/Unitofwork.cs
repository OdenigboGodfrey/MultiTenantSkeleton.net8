

using webapi_80.src.Shared.DatabaseContext;
using webapi_80.src.Tenant.SchemaTenant;
using webapi_80.src.User.Contract;
using webapi_80.src.User.Services;
// using webapi_80.src.Weather.Contracts;
// using webapi_80.src.Weather.Repositories.Weather;
// using webapi_80.src.Weather.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using webapi_80.src.Tenant.SchemaTenant.SchemaContext;
using webapi_80.src.Tenant.Contract;
using webapi_80.src.Tenant.Service;

namespace webapi_80.src.Shared.Contract
{
    public class Unitofwork : IUnitofwork
    {
        // IWeatherRepository weatherRepository;

        private readonly ApplicationDbContext subdomainSchemaContext;
        private readonly ApplicationDbContext publicSchemaContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantSchema tenantSchema;
        public readonly String subdomain;
        
        public Unitofwork(IHttpContextAccessor httpContextAccessor, ITenantSchema tenantSchema) {
            var _host = tenantSchema.ExtractSubdomainFromRequest(httpContextAccessor.HttpContext);
            subdomain = _host;
            this.subdomainSchemaContext = tenantSchema.getRequestContext(_host);
            _httpContextAccessor = httpContextAccessor;
            publicSchemaContext = new ApplicationDbContext(new DbContextSchema());
            this.tenantSchema = tenantSchema;
        }
        
        public IUserServices GetUserServices;
        public ITenantService GetTenantService;

        public IUserServices UserServices {
            get
            {
                if (this.GetUserServices == null) {
                    this.GetUserServices = new UserServices(this.publicSchemaContext, this.subdomainSchemaContext);
                }
                return this.GetUserServices;
            }
        }

        public ITenantService TenantServices {
            get
            {
                if (this.GetTenantService == null) {
                    this.GetTenantService = new TenantService(this.publicSchemaContext, this.subdomainSchemaContext, tenantSchema);
                }
                return this.GetTenantService;
            }
        }

        string IUnitofwork.subdomain => this.subdomain;
    }
}