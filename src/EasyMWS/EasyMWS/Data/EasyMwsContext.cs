using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MountainWarehouse.EasyMWS.Data
{
	internal class EasyMwsContext : DbContext
	{
		public EasyMwsContext()
		{
			CreateDatabaseWithMigrationsIfNotExists();
		}

		private void CreateDatabaseWithMigrationsIfNotExists()
		{
			try
			{
				ReportRequestCallbacks.FirstOrDefault(x => x.Id == 1);
			}
			catch (SqlException e)
			{
				Database.Migrate();
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
		}

		internal DbSet<ReportRequestCallback> ReportRequestCallbacks { get; set; }
	}
}
