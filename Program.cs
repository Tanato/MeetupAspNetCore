using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                            .AddCommandLine(args)
                            .Build();

            new WebHostBuilder()
                .UseKestrel()
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }

    public class Startup
    {
        
        public Startup()
        {
            using (var db = new FastDbContext())
            {
                db.Database.EnsureCreated();
            }
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFrameworkSqlite();
            services.AddDbContext<FastDbContext>();
            services.AddMvc();
        }
        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }

    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly FastDbContext db;
        public HomeController(FastDbContext db)
        {
            this.db = db;
        }

        [RouteAttribute("{value?}")]
        public IActionResult Get(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                db.Messages.Add(new Message { Content = value });
                db.SaveChanges();
            }

            return Content(string.Join(Environment.NewLine,
            db.Messages.OrderByDescending(x => x.Id).Select(x =>
            $"{x.Id} - {x.Content}")));
        }
    }

    public class FastDbContext : DbContext
    {
        public DbSet<Message> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlite("Filename=FastDb.db");
        }
    }

    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; }
    }
}
