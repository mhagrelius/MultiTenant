using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace MultiTenant
{
    public class TenantComponent : OwningComponentBase, IDisposable
    {
        [Inject]
        NavigationManager? NavManager { get; set; }


        [Inject] 
        IServiceScopeFactory? ScopeFactory { get; set; }

        [Inject] 
        protected IHttpContextAccessor? HttpContextAccessor { get; set; }

        [Inject]
        protected TenantMiddlewareConfiguration Config { get; set; }

        private IServiceScope? _scope;

        protected HttpContext? Context => HttpContextAccessor?.HttpContext;

        protected new bool IsDisposed { get; private set; }

        protected new IServiceProvider ScopedServices
        {
            get
            {
                if (ScopeFactory == null)
                {
                    throw new InvalidOperationException("Services cannot be accessed before the component is initialized.");
                }

                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                _scope ??= ScopeFactory.CreateScope();
                var provider = _scope.ServiceProvider;

                return GetTenantProvider(provider);
            }
        }

        private IServiceProvider GetTenantProvider(IServiceProvider provider)
        {
            var absoluteUri = new Uri(NavManager?.Uri ?? string.Empty);
            var path = absoluteUri.PathAndQuery;
            var query = QueryHelpers.ParseQuery(absoluteUri.Query);

            var pathParts = path.Split('/');
            string qsFacility;
            if (query.TryGetValue(Config.QuerySegment, out var queryFacility) && !string.IsNullOrWhiteSpace(queryFacility.ToString()))
            {
                qsFacility = queryFacility.ToString();
            }
            else
            {
                qsFacility = Config.DefaultTenant;
            }

            var tenantContainers = provider.GetServices<TenantContainer>().ToList();
            var pathMatch = pathParts.Length > 0
                ? tenantContainers?.FirstOrDefault(r => r?.Name.Equals(pathParts[1], StringComparison.InvariantCultureIgnoreCase) == true)
                : null;
            var qsMatch = tenantContainers?.FirstOrDefault(r => r?.Name.Equals(qsFacility, StringComparison.InvariantCultureIgnoreCase) == true);

            TenantContainer? matchingContainer = null;
            if(pathMatch != null)
            {
                matchingContainer = pathMatch;
            }
            else if (qsMatch != null)
            {
                matchingContainer = qsMatch;
            }
            if(!(matchingContainer is null))
            {
                return new CompositeServiceProvider(matchingContainer, provider);
            }

            return provider;
        }

        void IDisposable.Dispose()
        {
            if (!IsDisposed)
            {
                _scope?.Dispose();
                _scope = null;
                Dispose(disposing: true);
                IsDisposed = true;
            }
        }
    }
}
