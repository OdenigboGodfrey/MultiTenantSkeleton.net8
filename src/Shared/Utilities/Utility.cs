using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using webapi_80.src.Shared.ViewModels;

namespace webapi_80.src.Shared.Utilities
{
    public static class Utility
    {
        public static string prepareSubdomainName(string schemaName) {
            if (schemaName != null && schemaName.Contains('-')) {
                schemaName = schemaName.Replace('-', '_');
            }
            return schemaName;
        }
        public static async Task<Page<T>> ToPageListAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            var count = await query.CountAsync();
            int offset = (pageNumber - 1) * pageSize;
            var items = await query.Skip(offset).Take(pageSize).ToArrayAsync();
            return new Page<T>(items, count, pageNumber, pageSize);
        }
    }
}