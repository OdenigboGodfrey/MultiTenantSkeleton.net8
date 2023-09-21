using webapi_80.src.Shared.Utilities;

namespace webapi_80.src.Tenant.SchemaTenant.SchemaContext
{
    public interface IDbContextSchema
    {
        string Schema { get; }
    }

    public class DbContextSchema : IDbContextSchema
    {
        public string Schema { get; }
        public DbContextSchema(string schema = "dbo")
        {
            Schema = Utility.prepareSubdomainName(schema);
        }
    }
}