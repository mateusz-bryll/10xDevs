using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace TaskFlow.Modules.WorkItems;

public static class Endpoints
{
    public static IEndpointRouteBuilder UseWorkItemsModule(this IEndpointRouteBuilder builder)
    {
        
        var moduleEndpoints = builder.MapGroup("api/v1/work-items");
        
        
        
        return builder;
    }
}