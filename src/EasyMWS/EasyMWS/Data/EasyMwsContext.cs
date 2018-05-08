using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MountainWarehouse.EasyMWS.Data
{
	internal sealed class EasyMwsContext : DbContext
	{
		public EasyMwsContext()
		{
			CreateDatabaseWithMigrationsIfNotExists();
		}

		private void CreateDatabaseWithMigrationsIfNotExists()
		{
			try
			{
				TestDbConnection();
			}
			catch (Exception e)
			{
				if (e is SqlException || e is DbUpdateException)
				{
					Database.Migrate();
					return;
				}
				throw;
			}
		}

		private void TestDbConnection()
		{
			var testAnyEntitiesQuery = ReportRequestCallbacks.Any();
		}

		private readonly string _connectionString;
		public EasyMwsContext(string connectionString = null) : this()
		{
			_connectionString = connectionString;
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

			var conn = new SqlConnectionStringBuilder(_connectionString ?? configuration["connectionStrings:add:EasyMwsContext:connectionString"]);
			OverrideDefaultConnectionStringProperties(conn);

			optionsBuilder.UseSqlServer(conn.ToString(), opt => opt.EnableRetryOnFailure());
		}

		private void ConfigureDbContextForDotNetFramework(DbContextOptionsBuilder optionsBuilder)
		{
			var conn = new SqlConnectionStringBuilder(_connectionString ?? ConfigurationManager.ConnectionStrings["EasyMwsContext"].ConnectionString);
			OverrideDefaultConnectionStringProperties(conn);

			optionsBuilder.UseSqlServer(conn.ToString());
		}

		private void OverrideDefaultConnectionStringProperties(SqlConnectionStringBuilder connectionString)
		{
			connectionString.ConnectRetryCount = connectionString.ConnectRetryCount != 1 ? connectionString.ConnectRetryCount : 5;
			connectionString.ConnectRetryInterval = connectionString.ConnectRetryInterval != 10 ? connectionString.ConnectRetryInterval : 3;
			connectionString.MaxPoolSize = connectionString.MaxPoolSize != 100 ? connectionString.MaxPoolSize : 300;
			connectionString.ConnectTimeout = connectionString.ConnectTimeout != 15 ? connectionString.ConnectTimeout : 300;
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
		internal DbSet<AmazonReport> AmazonReports { get; set; }
	}
}
