using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
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
                tenantSchema._schema = origin;
                var schemaExists = await tenantSchema.DoesCurrentSubdomainExist();
                if (!schemaExists)
                {
                    var tenantCreated = await tenantSchema.NewTenant(tenantSchema._schema);
                    if (tenantCreated.ResponseCode == "201") Console.WriteLine($"tenantCreated {tenantCreated}");

                    // context.Response.StatusCode = 404;
                    // context.Response.ContentType = "application/json";
                    // var response = new {
                    //     ResponseCode = "404",
                    //     ResponseMessage = "Tenant Not found",
                    // };
                    // await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                    // return;

                }

                await next.Invoke();
            });
            return builder;
        }

        public static object ExecuteScalar(this DbContext context, string sql,
       List<DbParameter> parameters = null,
       CommandType commandType = CommandType.Text,
       int? commandTimeOutInSeconds = null)
        {
            Object value = ExecuteScalarFunc(context.Database, sql, parameters,
                                         commandType, commandTimeOutInSeconds);
            return value;
        }

        public static object ExecuteScalarFunc(this DatabaseFacade database,
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