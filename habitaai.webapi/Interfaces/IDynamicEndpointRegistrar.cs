public interface IDynamicEndpointRegistrar
{
    void MapDynamicEndpoint(string route, RequestDelegate handler);
}
