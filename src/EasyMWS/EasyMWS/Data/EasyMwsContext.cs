using System.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MountainWarehouse.EasyMWS.Data
{
	internal class EasyMwsContext : DbContext
	{
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			try
			{
				ConfigureDbContextForDotNetCore(optionsBuilder);
			}
			catch
			{
				ConfigureDbContextForDotNetFramework(optionsBuilder);
			}
		}

		private void ConfigureDbContextForDotNetCore(DbContextOptionsBuilder optionsBuilder)
		{
			IConfigurationRoot configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddXmlFile("App.config")
				.Build();

			optionsBuilder.UseSqlServer(configuration["connectionStrings:add:EasyMwsContext:connectionString"]);
		}

		private void ConfigureDbContextForDotNetFramework(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["EasyMwsContext"].ConnectionString);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ReportRequestCallback>()
				.HasIndex(e => new {e.RequestReportId, e.GeneratedReportId});
			modelBuilder.Entity<FeedSubmissionCallback>()
				.HasIndex(e => new { e.FeedSubmissionId });
		}

		internal DbSet<ReportRequestCallback> ReportRequestCallbacks { get; set; }
		internal DbSet<FeedSubmissionCallback> FeedSubmissionCallbacks { get; set; }
	}
}
