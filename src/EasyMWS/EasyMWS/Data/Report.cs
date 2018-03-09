using System.ComponentModel.DataAnnotations;

namespace MountainWarehouse.EasyMWS.Data
{
    public class Report
    {
		[Key]
	    public int Id { get; set; }

	    public string Name { get; set; }
    }
}
