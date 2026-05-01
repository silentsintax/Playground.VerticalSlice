using Microsoft.AspNetCore.Mvc;
using Playground.VerticalSlice.Api.Endpoints.Extensions;
using Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity;

namespace Playground.VerticalSlice.Api.Endpoints
{
    public static class FixedIncomeEndpoints
    {
        public static IEndpointRouteBuilder MapFixedIncomeEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup("/fixed-income")
                .WithTags("Fixed Income");


            group.MapPost("/", async (
                CreateSecurityRequest request,
                [FromServices] ICreateSecurityService service, CancellationToken ct) =>
            {
                return await service
                .Create(request, ct)
                .ToCreated(response => $"/api/fixed-income/{response.id}");

            })
            .WithName("CreateSecurity")
            .WithSummary("Create a new fixed-income security")
            .Produces<CreateSecurityResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
            return app;
        }

    }
}
