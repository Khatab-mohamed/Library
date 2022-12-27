using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Library.API.Services;
using Library.API.Entities;
using Microsoft.EntityFrameworkCore;
using Library.API.Dtos;
using Library.API.Helpers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Diagnostics;

namespace Library.API
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
            });

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<ILibraryRepository, LibraryRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
            ILoggerFactory loggerFactory, LibraryContext libraryContext)
        {
            //  Adding a console logger to the loggin system
            loggerFactory.AddConsole();

            
            loggerFactory.AddDebug(LogLevel.Information);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        var exceptionHandlerFeature= context.Features.Get<IExceptionHandlerFeature>();

                        if(exceptionHandlerFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global Exception logger");
                            logger.LogError(500,
                                exceptionHandlerFeature.Error,
                                exceptionHandlerFeature.Error.Message);
                        }
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexepected fault happnened, Try again later.");
                    });
                });
            }
            AutoMapper.Mapper.Initialize(config =>
            {
            config.CreateMap<Author, AuthorDto>()
            .ForMember(destination => destination.Name,
            option => option.MapFrom(src =>
            $"{src.FirstName} {src.LastName}"))
            .ForMember(destination => destination.Age,
            option => option.MapFrom(src => src.DateOfBirth.GetCurrentAge()));

            config.CreateMap<Book, BookDto>();
            config.CreateMap<AuthorCreationDto, Author>();
            config.CreateMap<BookCreationDto, Book>();
            config.CreateMap<BookUpdateDto, Book>();
            config.CreateMap<Book, BookUpdateDto>();
            });
            
            libraryContext.EnsureSeedDataForContext();


            app.UseMvc(); 
        }
    }
}
