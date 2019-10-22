using Microsoft.Extensions.DependencyInjection;
using System;

namespace MultiTenant
{
    public class TenantContainer
    {
        public string Name { get; set; }
        public readonly IServiceProvider Container;

        public TenantContainer(string tenant, IServiceCollection services)
        {
            Name = tenant;
            Container = services.BuildServiceProvider();
        }
    }
}
