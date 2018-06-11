using System.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MountainWarehouse.EasyMWS.Data
{
	internal sealed class EasyMwsContext : DbContext
	{
		public EasyMwsContext()
		{
			ApplyMigrationsAndCreateDatabaseIfNotExists();
		}

		private void ApplyMigrationsAndCreateDatabaseIfNotExists()
		{
			Database.Migrate();
		}

		private string _connectionString;
		public EasyMwsContext(string connectionString = null) : this()
		{
			_connectionString = connectionString;

			if (!string.IsNullOrEmpty(connectionString))
			{
				Database.GetDbConnection().ConnectionString = connectionString;
			}
		}

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

			optionsBuilder
				.UseLazyLoadingProxies()
				.UseSqlServer(_connectionString ?? configuration["connectionStrings:add:EasyMwsContext:connectionString"]);
		}

		private void ConfigureDbContextForDotNetFramework(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder
				.UseLazyLoadingProxies()
				.UseSqlServer(_connectionString ?? ConfigurationManager.ConnectionStrings["EasyMwsContext"].ConnectionString);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ReportRequestEntry>()
				.HasIndex(e => new {e.RequestReportId, e.GeneratedReportId});
			modelBuilder.Entity<FeedSubmissionEntry>()
				.HasIndex(e => new { e.FeedSubmissionId });
		}

		internal DbSet<ReportRequestEntry> ReportRequestEntries { get; set; }
		internal DbSet<FeedSubmissionEntry> FeedSubmissionEntries { get; set; }
	}
}
