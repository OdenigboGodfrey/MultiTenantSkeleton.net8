using System;
using webapi_80.src.Tenant.ViewModel;

namespace webapi_80.src.Tenant.Mapper {
public static class TenantMapper {
    public static Tenant.Model.Tenant Map(this TenantRegistrationVM model)
        {
            return new Tenant.Model.Tenant()
            {
                CompanyName = model.CompanyName,
                Subdomain = model.Subdomain
            };

        }

        public static TenantModelVM Map(this Tenant.Model.Tenant model)
        {
            return new TenantModelVM()
            {
                CompanyName = model.CompanyName,
                DateCreated = model.DateCreated,
                Id = model.Id,
                LastUpdate = model.LastUpdate,
                Subdomain = model.Subdomain,
                UpdatedBy = model.UpdatedBy,
                LogoUrl = model.LogoUrl,
            };

        }
}
}