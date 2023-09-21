using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;

namespace webapi_80.src.Tenant.SchemaTenant
{
    public static class Extensions
    {
        public static IApplicationBuilder CustomEnginerInterceptor(this IApplicationBuilder builder)
        {
            builder.Use(async (context, next) =>
            {
                TenantSchema tenantSchema = new TenantSchema();
                var origin = tenantSchema.ExtractSubdomainFromRequest(context);
                // tenantSchema.
                tenantSchema._schema = origin;
                var schemaExists = await tenantSchema.DoesCurrentSubdomainExist();
                Console.WriteLine($"context.Request.Headers {context.Request.Headers["Referer"]}, origin {origin}, tenantSchema._schema {tenantSchema._schema}, schemaExists {schemaExists}");
                if(!schemaExists) {
                    // tenant doesnt exist
                    var tenantCreated = await tenantSchema.NewTenant(tenantSchema._schema);
                    if (tenantCreated.ResponseCode == "201") Console.WriteLine($"tenantCreated {tenantCreated}");
                }

                
                // check schema if not exist on every request
                await next.Invoke();
            });
            return builder;
        }

        public static object ExecuteScalar(this DbContext context, string sql,
       List<DbParameter> parameters = null,
       CommandType commandType = CommandType.Text,
       int? commandTimeOutInSeconds = null)
        {
            Object value = ExecuteScalar(context.Database, sql, parameters,
                                         commandType, commandTimeOutInSeconds);
            return value;
        }

        public static object ExecuteScalar(this DatabaseFacade database,
        string sql, List<DbParameter> parameters = null,
        CommandType commandType = CommandType.Text,
        int? commandTimeOutInSeconds = null)
        {
            Object value;
            using (var cmd = database.GetDbConnection().CreateCommand())
            {
                if (cmd.Connection.State != ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }
                cmd.CommandText = sql;
                cmd.CommandType = commandType;
                if (commandTimeOutInSeconds != null)
                {
                    cmd.CommandTimeout = (int)commandTimeOutInSeconds;
                }
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }
                value = cmd.ExecuteScalar();
            }
            return value;
        }

    }
}