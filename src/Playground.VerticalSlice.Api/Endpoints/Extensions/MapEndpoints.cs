namespace Playground.VerticalSlice.Api.Endpoints.Extensions
{
    public static class MapEndpoints
    {
        public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapFixedIncomeEndpoints();
            return app;
        }
    }
}
