using System.Collections.Generic;
using Dapper.ColumnMapper;
using DapperTestApp.DbCommon.DbContracts.Outputs;
using DapperTestApp.DbCommon.Options;
using DapperTestApp.DbCommon.Repositories;
using DapperTestApp.DbCommon.Sessions;
using DapperTestApp.DbCommon.TypeHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace DapperTestApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<DbConnectionOption>(Configuration.GetSection(
                DbConnectionOption.DbConnectionStrings));

            services.AddScoped<IRetailDbSession, RetailDbSession>();
            services.AddTransient<IShipmentProductRepository, ShipmentProductRepository>();

            ColumnTypeMapper.RegisterForTypes(
                typeof(Product), typeof(Shipment), typeof(ProductShipments));

            Dapper.SqlMapper.AddTypeHandler(new JsonDocumentTypeHandler());
            Dapper.SqlMapper.AddTypeHandler(typeof(IEnumerable<Shipment>), new JsonTypeHandler());

            services.AddControllers();
            services.AddSwaggerGen(c => c.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "DapperTestApp",
                    Version = "v1"
                })
            );
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IOptions<DbConnectionOption> dbConnectionOptions)
        {
            dbConnectionOptions.Value.Validate();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DapperTestApp v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
                endpoints.MapControllers());
        }
    }
}
