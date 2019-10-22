using Microsoft.Extensions.DependencyInjection;
using System;

namespace MultiTenant
{
    public class CompositeServiceProvider : IServiceProvider, ISupportRequiredService
    {
        private readonly TenantContainer tenantContainer;
        private readonly IServiceProvider fallback;
        public CompositeServiceProvider(TenantContainer tenantContainer, IServiceProvider fallbackContainer)
        {
            this.tenantContainer = tenantContainer ?? throw new ArgumentNullException(nameof(tenantContainer));
            this.fallback = fallbackContainer ?? throw new ArgumentNullException(nameof(fallbackContainer));
        }

        public object GetRequiredService(Type serviceType)
        {
            return this.GetService(serviceType)
                ?? throw new InvalidOperationException(
                    $"Unable to locate an instance of required service '{serviceType}'.");
        }

        public object? GetService(Type serviceType)
        {
            var result = tenantContainer?.Container?.GetService(serviceType);
            return result is null ? fallback?.GetService(serviceType) : result;
        }
    }
}
