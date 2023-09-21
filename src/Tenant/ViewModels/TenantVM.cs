using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace webapi_80.src.Tenant.ViewModel
{
    public class TenantModelVM
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; }
        public string ContactNo { get; set; }
        // public string Location { get; set; }
        public string Subdomain { get; set; }
        public DateTime DateCreated { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime LastUpdate { get; set; }
        public string LogoUrl { get; set; }
    }

    public class TenantVM
    {
        public string CompanyName { get; set; }
        public string ContactNo { get; set; }
        // public string Location { get; set; }
        public string Subdomain { get; set; }
        public string? LogoUrl { get; set; }
        public string CompanySize{ get; set; }
    }

    public class TenantRegistrationVM
    {
        public string CompanyName { get; set; }
        public string ContactNo { get; set; }
        // public string Location { get; set; }
        public string Subdomain { get; set; }
        public string CompanySize{ get; set; }
    }

    public class RegisterTenantVM
    {
        // public IFormFile Logo { get; set; }
        public TenantRegistrationVM tenant { get; set; }
        // public CompanyAdminModel companyAdmin  { get; set; }
    }
    public class TenantMigrationResultVM 
    {
        public int TotalSchemas { get; set; } = 0;
        public int MigratedSchemas { get; set; } = 0;
        public List<string> FailedSchemaMigrations { get; set; }
    }
}
