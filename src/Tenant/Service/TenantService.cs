using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using webapi_80.src.Shared.DatabaseContext;
using webapi_80.src.Shared.Utilities;
using webapi_80.src.Shared.ViewModels;
using webapi_80.src.Tenant.Contract;
using webapi_80.src.Tenant.Mapper;
using webapi_80.src.Tenant.SchemaTenant;
using webapi_80.src.Tenant.SchemaTenant.SchemaContext;
using webapi_80.src.Tenant.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using webapi_80.src.User.Models;

namespace webapi_80.src.Tenant.Service
{
    public class TenantService : ITenantService
    {
        private ApplicationDbContext Db;
        private ApplicationDbContext subdomainSchemaContext;

        private readonly ITenantSchema tenantSchema;
        public TenantService(ApplicationDbContext publicSchemaContext, ApplicationDbContext subdomainSchemaContext, ITenantSchema tenantSchema)
        {
            this.Db = publicSchemaContext;
            this.subdomainSchemaContext = subdomainSchemaContext;
            this.tenantSchema = tenantSchema;
        }


        public async Task<TenantModelVM> GetTenantById(Guid id)
        {
            var tenantInfo = Db.Tenants.FirstOrDefault(x => x.Id == id);
            if (tenantInfo == null) throw new Exception("Tenant not found");
            int result = await Db.SaveChangesAsync();
            return tenantInfo.Map();
        }

        public async Task<TenantModelVM> GetTenantBySubdomain(string subdomain)
        {
            var tenantInfo = Db.Tenants.FirstOrDefault(x => x.Subdomain == subdomain);
            if (tenantInfo == null) throw new Exception("Tenant not found");
            int result = await Db.SaveChangesAsync();
            return tenantInfo.Map();
        }
        public async Task<ApiResponse<bool>> RegisterTenant(RegisterTenantVM model)
        {
            var response = new ApiResponse<bool>
            {
                Data = false,
                ResponseCode = "400",
                ResponseMessage = ""
            };
            bool isFromSocialOauth = false;
            if (Db.Tenants.Where(x => x.Subdomain == Utility.prepareSubdomainName(model.tenant.Subdomain)).Count() > 0)
            {
                response.ResponseCode = "409";
                response.ResponseMessage = "Subdomain already exists.";
                return response;
            }
            // if (Db.Tenants.Where(x => x.CompanyName == model.tenant.CompanyName).Count() > 0) {
            //     response.ResponseMessage = "Company already exists.";
            // }
            if (!string.IsNullOrEmpty(model.tenant.ContactNo))
            {
                response.ResponseCode = "409";
                response.ResponseMessage = "Company contact number already exists.";
                return response;
            }

            // create tenant
            var tenant = model.tenant.Map();
            // remove special chars from subdomain
            tenant.Subdomain = Utility.prepareSubdomainName(tenant.Subdomain);
            tenant.DateCreated = DateTime.Now;
            // create subdomain schema
            var newSchemaCreated = await tenantSchema.NewSchema(model.tenant.Subdomain);
            // apply migrations
            if (!newSchemaCreated)
            {
                response.ResponseMessage = "Something went wrong while creating schema. ";
                response.ResponseCode = "500";
                return response;
            }
            tenant.isSchemaCreated = true;

            var migrationStatus = await tenantSchema.RunMigrations(tenant.Subdomain);
            if (!migrationStatus)
            {
                response.ResponseMessage = "Something went wrong while running migrations on schema.";
                response.ResponseCode = "500";
                return response;
            }
            tenant.LastMigration = DateTime.Now;
            Db.Tenants.Add(tenant);

            int result = await Db.SaveChangesAsync();
            response.Data = result > 0 ? true : false;
            if (response.Data) {
                response.ResponseMessage = "Tenant information saved";
                response.ResponseCode = "201";
            } else {
                response.ResponseMessage = "Tenant information failed to be saved";
                response.ResponseCode = "500";
            }
            
            
            return response;
            // return result > 0 ? true : false;
        }
        
        public async Task<TenantMigrationResultVM> MigrateTenants()
        {
            // get all tenants subdomain
            // change context for each and apply migrations to each or call the appy migrations function
            // 
            Console.WriteLine("Running migration");
            var response = new TenantMigrationResultVM();
            // var tenantSchemaInstance = new TenantSchema();
            var tenantSchemaInstance = tenantSchema;
            List<string> tenants = new List<string>();
            try {
                tenants = Db.Tenants.Where(x => x.isSchemaCreated).Select(x => x.Subdomain).ToList();
            } catch(Exception ex) {
                // on first run, tenants table would not exist and would throw an error
            }
             
            response.FailedSchemaMigrations = new List<string>();
            // run for public schema
            var migrationPublicSchema = await tenantSchemaInstance.RunMigrations("dbo");
            if (!migrationPublicSchema)
            {
                throw new Exception("Something went wrong while running migrations on the public schema");
            }
            response.TotalSchemas++;
            response.MigratedSchemas++;
            for (int i = 0; i < tenants.Count(); i++)
            {
                var currentSchema = tenants[i];
                response.TotalSchemas++;
                try
                {
                    var result = await tenantSchemaInstance.RunMigrations(currentSchema);
                    if (result) response.MigratedSchemas++;
                    else
                    {
                        response.FailedSchemaMigrations.Add(currentSchema);
                    }

                }
                catch (Exception ex)
                {
                    response.FailedSchemaMigrations.Add(currentSchema);
                    Console.WriteLine(ex);
                    continue;
                }
            }

            return response;
        }

        public async Task<ApiResponse<bool>> DeleteTenant(string subdomain)
        {
            var response = new ApiResponse<bool> {
                Data=false,
                ResponseCode="400",
                ResponseMessage=""
            };
            // create relationship between user and company
            Model.Tenant tenant = Db.Tenants.FirstOrDefault(x => x.Subdomain == subdomain);
            if (tenant == null) 
            {
                response.ResponseMessage = "Tenant not found.";
                return response;
            }

            // remove admin
            var admin = Db.Users.FirstOrDefault(x => x.Id == new Guid(tenant.AdminId));
            if (admin == null) {
                response.ResponseMessage = "Tenant admin user not found.";
                return response;
            }

            var result = await Db.SaveChangesAsync();
            response.Data = result > 0;
            response.ResponseMessage = (result > 0) ? "Tenant information removed" : "Failed to remove tenant information";
            response.ResponseCode = (result > 0) ? "200" : "500";
            return response;
        }

    }
}
