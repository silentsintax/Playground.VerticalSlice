using Microsoft.AspNetCore.Mvc;
using Playground.VerticalSlice.Api.Endpoints.Extensions;
using Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity;
using Playground.VerticalSlice.Application.Features.FixedIncome.SearchSecurity;

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


            group.MapGet("/by-id", async (
                int id,
                [FromServices] ISearchSecurityService service, CancellationToken ct) =>
            {
                return await service
                .GetByIdAsync(id, ct)
                .ToOk();

            })
            .WithName("GetSecurityById")
            .WithSummary("Get the security by identification")
            .Produces<SecurityDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);


            group.MapGet("/by-isin", async (
                string isin,
                [FromServices] ISearchSecurityService service, CancellationToken ct) =>
            {
                return await service
                .GetByIsinAsync(isin, ct)
                .ToOk();

            })
            .WithName("GetSecurityByIsin")
            .WithSummary("Get the security by ISIN")
            .Produces<SecurityDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            return app;
        }

    }
}
