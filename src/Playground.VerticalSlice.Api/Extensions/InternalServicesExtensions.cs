using Dapper;
using FluentValidation;
using Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity;
using Playground.VerticalSlice.Application.Shared.Helpers;
using System.Text.Json.Serialization;

namespace Playground.VerticalSlice.Api.Extensions
{
    public static class InternalServicesExtensions
    {
        public static IServiceCollection AddInternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
            services.AddScoped<IValidator<CreateSecurityRequest>, FixedIncomeSecurityValidator>();
            services.AddScoped<ICreateSecurityService, CreateSecurityService>();
            services.AddScoped<ICreateSecurityRepository>(_ => new CreateSecurityRepository(configuration.GetConnectionString("DefaultConnection")!));

            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.SerializerOptions.Converters.Add(new DateOnlyJsonConverter());
            });

            return services;
        }
    }
}
