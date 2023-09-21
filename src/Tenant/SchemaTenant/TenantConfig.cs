using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using webapi_80.src.Shared.DatabaseContext;
using webapi_80.src.Shared.Utilities;
using webapi_80.src.Tenant.SchemaTenant.SchemaContext;

namespace webapi_80.src.Tenant.SchemaTenant
{

    public interface ITenantConfig<T>
    {
        T getRequestContext(string hostURL);
        public string getSubdomainName(string domainURL);
    }

    public class TenantConfig : ITenantConfig<ApplicationDbContext>
    {
        ApplicationDbContext publicSchemaContext;
        string conString;
        private readonly IConfiguration _config;

        public TenantConfig()
        {
            _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            conString = _config.GetSection("ConnectionStrings")["MultiTenantBlog"];
            this.publicSchemaContext = new ApplicationDbContext(conString, new DbContextSchema());
        }

        public string getSubdomainName(string domainURL)
        {
            string domainName = domainURL;
            domainName = domainName.Split(":")[0];
            var _schemaName = domainName.Split(".")[0];
            _schemaName = Utility.prepareSubdomainName(_schemaName);
            Console.WriteLine("domain name gotten from schema " + _schemaName);
            return _schemaName;
        }

        public ApplicationDbContext getRequestContext(string hostURL)
        {
            string rootDomain = _config["RootDomain"];
            var subdomainUrl = this.getSubdomainName(hostURL);
            Tenant.Model.Tenant tenantSubdomain = null;
            Console.WriteLine("rootDomain " + rootDomain + " " + subdomainUrl);
            // || 
            if (subdomainUrl == "api" || subdomainUrl == "admin")
            {
                subdomainUrl = "dbo";
                tenantSubdomain = new Tenant.Model.Tenant();
            } else {
                // verify schema exists in subdomain
                tenantSubdomain = this.publicSchemaContext.Tenants.FirstOrDefault(x => x.Subdomain == subdomainUrl);
            }
            
            if (tenantSubdomain != null)
            {
                return new ApplicationDbContext(conString, new DbContextSchema(subdomainUrl));
            }
            else
            {
                if (subdomainUrl == rootDomain) return null;
                // not existing
                throw new Exception("Subdomain not found.");
            }
        }
    }
}