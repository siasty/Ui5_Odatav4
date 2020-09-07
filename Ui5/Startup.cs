using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Ui5.Controllers;
using Ui5.Data;

namespace Ui5
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
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

            services.AddControllers(mvcOptions => mvcOptions.EnableEndpointRouting = false);

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

            services.AddOData();
            services.AddODataQueryFilter();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "/")),
                RequestPath = "/ui5"
            });

            app.UseRouting();

            app.UseDefaultFiles();

            app.UseODataBatching();

            //app.Use((context, next) =>
            //{
            //    context.Response.Headers["OData-Version"] = "4.0";
            //    return next.Invoke();
            //});

            app.UseCors();




            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".properties"] = "application/text";

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "")),
                RequestPath = "",
                ContentTypeProvider = provider
            });

            app.UseMvc(routes =>
            {
                routes.Select().Expand().Filter().OrderBy().MaxTop(null).Count().EnableContinueOnErrorHeader();
                routes.MapODataServiceRoute("odata", "odata", GetEdmModel(), new DefaultODataBatchHandler());
                routes.EnableDependencyInjection();
            });


            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetRequiredService<TestDbContext>();

                if (db.Database.EnsureCreated())
                {
                    if (!db.Suppliers.Any())
                    {
                        db.Suppliers.AddRange(new List<Supplier> {
                        new Supplier { Id = 1, Name = "Stonka", Products =  new List<Product> {CreateNewProduct(1,"Cola", 130, "drink",1), CreateNewProduct(2,"Fanta", 140, "drink", 1) } },
                        new Supplier { Id = 2, Name = "Biedronka", Products =  new List<Product> {CreateNewProduct(3,"Pepsi", 130, "drink",2), CreateNewProduct(4,"Sprajt", 40, "drink", 2) } },

                         });
                        db.SaveChanges();
                    }
                }
            }



        }

        IEdmModel GetEdmModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            odataBuilder.EntitySet<Product>("Products").EntityType.Filter().Count().Expand().OrderBy().Page().Select();
            odataBuilder.EntitySet<Supplier>("Suppliers").EntityType.Filter().Count().Expand().OrderBy().Page().Select(); ;

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
