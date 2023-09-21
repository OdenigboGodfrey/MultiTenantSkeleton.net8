using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using webapi_80.src.Tenant.SchemaTenant.SchemaContext;
using webapi_80.src.Shared.Utilities;
using webapi_80.src.Shared.ViewModels;
using System.Linq;
using webapi_80.src.Shared.DatabaseContext;

namespace webapi_80.src.Tenant.SchemaTenant
{

    public interface ITenantSchema
    {
        ApplicationDbContext getRequestContext();
        ApplicationDbContext getRequestContext(string hostURL);
        Task<bool> DoesCurrentSubdomainExist();
        Task<bool> RunMigrations(string schemaName);
        Task<bool> NewSchema(string schemaName);
        string ExtractSubdomainFromRequest(HttpContext httpContext);
    }

    public class TenantSchema : ITenantSchema
    {
        public string _schema;
        private string conString;

        public ApplicationDbContext context;
        private readonly IConfiguration _config;


        public TenantSchema()
        {
            _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            conString = _config.GetSection("ConnectionStrings")["MultiTenantBlog"];
            this.context = new ApplicationDbContext(conString, new DbContextSchema());

        }
        public TenantSchema(string hostURL)
        {
            _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            // open new connection
            conString = _config.GetSection("ConnectionStrings")["MultiTenantBlog"];
            this.context = new ApplicationDbContext(conString, new DbContextSchema());
            // create new schema if not exist
            _schema = getSubdomainName(hostURL);
        }



        public async Task<bool> NewSchema(string schemaName)
        {
            schemaName = Utility.prepareSubdomainName(schemaName);

            var _schemaExist = (string)context.ExecuteScalar($"SELECT name FROM sys.schemas where name = '{schemaName}';");
            if (string.IsNullOrEmpty(_schemaExist))
            {
                // doesnt exist
                // create 
                await this.context.Database.ExecuteSqlRawAsync($"CREATE SCHEMA {schemaName};");
                _schemaExist = (string)context.ExecuteScalar($"SELECT name FROM sys.schemas where name = @paramName;", new List<DbParameter>() { new Microsoft.Data.SqlClient.SqlParameter("@paramName", schemaName) });
            }

            return string.IsNullOrEmpty(_schemaExist) ? false : true;
        }

        private async Task<int> ExecSQL(string sql)
        {
            return (int)await this.context.Database.ExecuteSqlRawAsync(sql);
        }

        private T ExecScalar<T>(string sql, List<DbParameter> parameters)
        {
            return (T)context.ExecuteScalar(sql, parameters);
        }

        public string getSubdomainName(string domainURL)
        {
            string domainName = domainURL;
            domainName = domainName.Replace("http://", "").Replace("https://", "");
            
            domainName = domainName.Split(":")[0];
            var _schemaName = domainName.Split(".")[0];
            _schemaName = Utility.prepareSubdomainName(_schemaName);
            return _schemaName;
        }

        public async Task<bool> RunMigrations(string schemaName)
        {
            try
            {
                schemaName = Utility.prepareSubdomainName(schemaName);
                var schema = await this.NewSchema(schemaName);
                if (!schema) throw new Exception("Schema not created.");
                var _context = new ApplicationDbContext(conString, new DbContextSchema(schemaName));
                _context.Database.Migrate();
                Console.WriteLine("db migrated");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<ApiResponse<bool>> NewTenant(string Subdomain)
        {
            var response = new ApiResponse<bool>
            {
                Data = false,
                ResponseCode = "400",
                ResponseMessage = ""
            };
            var preparedSubdomainName = Utility.prepareSubdomainName(Subdomain);
            try {
                // on first run, 
                Console.WriteLine($"new tenant run");
                if (this.context.Tenants.Where(x => x.Subdomain == preparedSubdomainName).Count() > 0)
                {
                    response.ResponseCode = "409";
                    response.ResponseMessage = "Subdomain already exists.";
                    return response;
                }
            }
            catch(Exception ex) {
                // would fail for first run
                //throw ex;
            }

            // create tenant
            var tenant = new Model.Tenant(); //model.tenant.Map();
            // remove special chars from subdomain
            tenant.Subdomain = preparedSubdomainName;
            Console.WriteLine($"origin Subdomain {Subdomain} + preparedSubdomainName {preparedSubdomainName} + {tenant.Subdomain}");
            tenant.DateCreated = DateTime.Now;
            // create subdomain schema
            var newSchemaCreated = await this.NewSchema(tenant.Subdomain);
            // apply migrations
            if (!newSchemaCreated)
            {
                response.ResponseMessage = "Something went wrong while creating schema. ";
                response.ResponseCode = "500";
                return response;
            }
            tenant.isSchemaCreated = true;

            var migrationStatus = await this.RunMigrations(tenant.Subdomain);
            if (!migrationStatus)
            {
                response.ResponseMessage = "Something went wrong while running migrations on schema.";
                response.ResponseCode = "500";
                return response;
            }
            tenant.LastMigration = DateTime.Now;
            this.context.Tenants.Add(tenant);

            int result = await this.context.SaveChangesAsync();
            response.Data = result > 0 ? true : false;
            if (response.Data) {
                response.ResponseMessage = "Tenant information saved";
                response.ResponseCode = "201";
            } else {
                response.ResponseMessage = "Tenant information failed to be saved";
                response.ResponseCode = "500";
            }
            return response;
        }
       

        public ApplicationDbContext getRequestContext()
        {
            return this.context;
        }

        public ApplicationDbContext getRequestContext(string hostURL)
        {
            string rootDomain = _config["RootDomain"];
            var subdomainUrl = this.getSubdomainName(hostURL);
            if (subdomainUrl == "api" || subdomainUrl == "admin" || subdomainUrl == rootDomain)
            {
                return new ApplicationDbContext(conString, new DbContextSchema());
            } else {
                return new ApplicationDbContext(conString, new DbContextSchema(getSubdomainName(hostURL)));
            }
        }

        public async Task<bool> DoesCurrentSubdomainExist()
        {
            try {
                string rootDomain = _config["RootDomain"];
                if (_schema == "api" || _schema == "admin" || _schema == rootDomain)
                {
                    // public subdomains
                    // i.e subdomains which would use dbo as its schema mainly used for public functionalities
                    return true;
                }
                var tenant = await this.context.Tenants.FirstOrDefaultAsync(x => x.Subdomain == _schema);
                if (tenant == null) return false;
                Console.WriteLine($"tenant ${tenant.Subdomain}");
                return true;
            } catch(Exception ex) {
                return false;
            }
        }

        public string ExtractSubdomainFromRequest(HttpContext httpContext)
        {

            var _host = httpContext.Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(_host))
            {
                _host = httpContext.Request.Host.ToString();
            }

            _host = getSubdomainName(_host);

            return _host;
        }

    }

}