using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Ui5_Backend.Controllers;
using Ui5_Backend.Data;

namespace Ui5_Backend
{
    public class Startup
    {

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;

            });
            services.Configure<IISOptions>(options =>
            {
                options.ForwardClientCertificate = false;
            });

            services.AddDbContext<TestDbContext>(opt => opt.UseInMemoryDatabase("Test"));

            services.AddCors(options =>
            {
                options.AddPolicy(
                     "AllowAllOrigins",
                      builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("OData-Version")
                        );
            });

            var batchHandler = new DefaultODataBatchHandler();
            services.AddControllers()
            .AddOData(opt => opt
                .AddRouteComponents("odata", GetEdmModel(),batchHandler)
                .Count().Filter().Expand().Select().OrderBy().SetMaxTop(100)
                .EnableQueryFeatures()
            );

            services.AddCors(options =>
            {
                options.AddPolicy(
                     "AllowAllOrigins",
                      builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        );

                options.DefaultPolicyName = "AllowAllOrigins";
            });


            services.AddODataQueryFilter();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseODataRouteDebug();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            
            app.UseMyOdataMiddleware();
            app.Use((context, next) =>
            {
                 context.Response.Headers["OData-Version"] = "4.0";  
                 context.Response.Headers["OData-MaxVersion"] = "4.0";      
                 Console.WriteLine(context.Request.Path);
                 if (context.Request.Path.ToString().Contains("/odata/$batch"))
                 {
                    context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                    context.Response.Headers["Access-Control-Allow-Headers"]= "*";
                    context.Response.Headers["Access-Control-Expose-Headers"]= "OData-Version";

                }
                return next.Invoke();
            });

            app.UseODataQueryRequest();
            app.UseODataBatching();
            app.UseRouting();
            app.UseCors("AllowAllOrigins");
    
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetRequiredService<TestDbContext>();

                if (db.Database.EnsureCreated())
                {
                    if (!db.Suppliers.Any())
                    {
                        db.Suppliers.AddRange(new List<Supplier> {
                        new Supplier { Id = 1, Name = "Supplier1", Products =  new List<Product> { CreateNewProduct(1,"Product1", 13, "Cat1",1), CreateNewProduct(2,"Product2", 140, "Cat1", 1) } },
                        new Supplier { Id = 2, Name = "Supplier1", Products =  new List<Product> {CreateNewProduct(3,"Product3", 130, "Cat1",2), CreateNewProduct(4,"Product4", 40, "Cat2", 2) } },

                         });
                        db.SaveChanges();
                    }
                }
            }



        }

        IEdmModel GetEdmModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            odataBuilder.EntitySet<Product>("Products");
            odataBuilder.EntitySet<Supplier>("Suppliers");

            odataBuilder.Namespace = "ProductService";
            odataBuilder.EntityType<Product>().Action("Rate").Parameter<int>("Rating");
            odataBuilder.EntityType<Product>().Collection.Function("MostExpensive").Returns<double>();

            return odataBuilder.GetEdmModel();
        }

        public static Product CreateNewProduct(int id, string name, decimal price, string categ, int SupplierId)
        {
            return new Product
            {
                Id = id,
                Name = name,
                Price = price,
                Category = categ,
                SupplierId = SupplierId
            };
        }

    }
}
