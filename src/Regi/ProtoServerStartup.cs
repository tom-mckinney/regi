using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Regi.Services.Grpc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi
{
    public class ProtoServerStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<HandshakeService>();
                endpoints.MapGrpcService<GreeterService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
