namespace MultiTenant
{
    public class TenantMiddlewareConfiguration
    {
        public string DefaultTenant { get; set; } = string.Empty;
        public string QuerySegment { get; set; } = "tenant";

        public int PathSegment { get; set; } = 1;
    }
}