public class DynamicEndpointRegistrar : IDynamicEndpointRegistrar
{
    private readonly IEndpointRouteBuilder _endpoints;

    public DynamicEndpointRegistrar(IEndpointRouteBuilder endpoints)
    {
        _endpoints = endpoints;
    }

    public void MapDynamicEndpoint(string route, RequestDelegate handler)
    {
        _endpoints.Map(route, handler);
    }
}
