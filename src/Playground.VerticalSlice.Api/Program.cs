using Playground.VerticalSlice.Api.Endpoints.Extensions;
using Playground.VerticalSlice.Api.Extensions;
using Scalar.AspNetCore;

namespace Playground.VerticalSlice.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddInternalServices(builder.Configuration);
            builder.Services.AddOpenApi();

            var app = builder.Build();

           
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.MapAllEndpoints();

            app.Run();
        }
    }
}
