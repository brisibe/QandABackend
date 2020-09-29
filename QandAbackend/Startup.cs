using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DbUp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QandAbackend.Data;
using QandAbackend.Hubs;

namespace QandAbackend
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

            //gets connection string from appsettings.json file and creates the database if it doesnt exist.
            var connectionString = Configuration.GetConnectionString("DefaultConnectionString");
            EnsureDatabase.For.SqlDatabase(connectionString);


            //we've told dbup where the database is and to look for sql scripts that have been embedded in our project. we've also told dbup to do the database migrations in a transaction.
            var upgrader = DeployChanges.To.SqlDatabase(connectionString, null).WithScriptsEmbeddedInAssembly(
                System.Reflection.Assembly.GetExecutingAssembly()
                ).WithTransaction().Build();

            //this gets dbup to do a database migration if there are any pending sql scripts.
            if (upgrader.IsUpgradeRequired())
            {
                upgrader.PerformUpgrade();
            }


            services.AddControllers();

            //This tells asp.net core that whenever IDataRepository is referenced in a constructor, substitute an instance of the DataRepository class.
            // AddScoped - Generates only one instance of the class in a given HTTP Request
            // AddTransient - Generates a new instance of the class each time it is requested
            // AddSingleton - Generates only one instance for the lifetime of the whole app.
            services.AddScoped<IDataRepository, DataRepository>();

            //enabling CORS policy which allows the api to be accessed by http://localhost:3000 frontend
            services.AddCors(options => options.AddPolicy("CorsPolicy", builder => builder.AllowAnyMethod().AllowAnyHeader().WithOrigins("http://localhost:3000").AllowCredentials()));
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("CorsPolicy");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                //SignalR requests to the /questionshub path will be handled by the QuestionsHub class
                endpoints.MapHub<QuestionsHub>("/questionshub");
            });
        }


        //private void PerformScriptUpdate()
        //{
        //    var connString = Configuration["ConnectionStrings:PersonnelDbConnectionString"];
        //    var upgraderTran = DeployChanges.To
        //        .SqlDatabase(connString)
        //        .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(),
        //            name => name.StartsWith("School.Personnel.Management.Scripts"))
        //        .LogToConsole()
        //        .Build();

        //    var resultEnt = upgraderTran.PerformUpgrade();
        //    if (!resultEnt.Successful)
        //    {
        //        Console.ForegroundColor = ConsoleColor.Red;
        //        Console.WriteLine(resultEnt.Error);
        //        Console.ResetColor();
        //    }
        //}
    }
}
