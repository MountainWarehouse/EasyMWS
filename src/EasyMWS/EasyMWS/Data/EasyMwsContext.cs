using System.Configuration;
using Microsoft.EntityFrameworkCore;

namespace MountainWarehouse.EasyMWS.Data
{
	public class EasyMwsContext : DbContext
	{
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["EasyMwsContext"].ConnectionString);
		}

		public DbSet<Report> Reports { get; set; }
	}
}
