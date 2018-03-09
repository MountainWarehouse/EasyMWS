using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MountainWarehouse.EasyMWS.Data
{
	public class EasyMwsContext : DbContext
	{
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			IConfigurationRoot configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddXmlFile("App.config")
				.Build();
			optionsBuilder.UseSqlServer(configuration["connectionStrings:add:EasyMwsContext:connectionString"]);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ReportRequestCallback>()
				.HasIndex(e => new {e.RequestReportId, e.GeneratedReportId});
		}

		public DbSet<ReportRequestCallback> ReportRequestCallbacks { get; set; }
	}
}
