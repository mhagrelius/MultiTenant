using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenant
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate next;

        public TenantMiddleware(RequestDelegate next, TenantMiddlewareConfiguration config)
        {
            this.next = next;
            Config = config ?? new TenantMiddlewareConfiguration();
        }

        public TenantMiddlewareConfiguration Config { get; }

        public async Task Invoke(HttpContext context, IEnumerable<TenantContainer> containers)
        {
            if (context.Request.Path.HasValue)
            {
                var pathParts = context.Request.Path.Value.Split('/');
                var qsFacility = context.Request.Query[Config.QuerySegment].ToString() == "" 
                    ? Config.DefaultTenant
                    : context.Request.Query[Config.QuerySegment].ToString();

                var tenantContainers = containers.ToList();
                var pathMatch = pathParts.Length > 0 
                    ? tenantContainers.FirstOrDefault(r => r.Name.Equals(pathParts[Config.PathSegment], StringComparison.InvariantCultureIgnoreCase)) 
                    : null;
                var qsMatch = tenantContainers.FirstOrDefault(r => r.Name.Equals(qsFacility, StringComparison.InvariantCultureIgnoreCase));

                if (pathMatch != null || qsMatch != null)
                {
                    context.RequestServices = new CompositeServiceProvider(pathMatch ?? qsMatch, context.RequestServices);
                }
            }

            await next(context);
        }
    }
}
