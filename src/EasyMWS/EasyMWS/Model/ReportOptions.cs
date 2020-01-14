using System.Collections.Generic;
using System.Text;

namespace MountainWarehouse.EasyMWS.Model
{
    public class ReportOptions
    {

	    public List<(string Name, string Value)> Options { get; set; } = new List<(string, string)>();

	    public void AddStringOption(string optionName, string optionValue)
	    {
		    Options.Add((optionName, optionValue));
	    }

	    public void AddBooleanOption(string optionName, bool optionValue)
	    {
		    Options.Add((optionName, optionValue ? "true" : "false"));
	    }

	    public void AddIntegerOption(string optionName, int optionValue)
	    {
		    Options.Add((optionName, $"{optionValue}"));
	    }

		public string GetOptionsString()
		{
			//TODO: figure out if anything needs to be encoded first
			var optionsSb = new StringBuilder();
			foreach (var option in Options)
			{
				optionsSb.Append($"{option.Name}={option.Value};");
			}
			return optionsSb.ToString();
		}
    }
}
