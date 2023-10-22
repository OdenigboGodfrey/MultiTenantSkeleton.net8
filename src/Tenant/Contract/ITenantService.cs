using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using webapi_80.src.Shared.ViewModels;
using webapi_80.src.Tenant.ViewModel;
using webapi_80.src.User.Models;

namespace webapi_80.src.Tenant.Contract
{
    public interface ITenantService
    {
        public Task<TenantModelVM> GetTenantById(Guid id);
        public Task<ApiResponse<bool>> RegisterTenant(RegisterTenantVM model);
        Task<TenantMigrationResultVM> MigrateTenants();
        Task<ApiResponse<bool>> DeleteTenant(string subdomain);
        Task<TenantModelVM> GetTenantBySubdomain(string subdomain);
        public Task<Page<TenantModelVM>> GetAllTenants(int pageNumber = 1, int pageSize = 20, string searchparam = null);
    }
}
