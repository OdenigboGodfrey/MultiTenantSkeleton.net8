using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace webapi_80.src.Tenant.SchemaTenant.SchemaContext
{
    public class DbSchemaAwareModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime) {
            var returnValue = context is IDbContextSchema schema
            ? (context.GetType(), schema.Schema, designTime)
            : (object)context.GetType();

            return returnValue;
        }

        public object Create(DbContext context) => Create(context, false);
    }
}
