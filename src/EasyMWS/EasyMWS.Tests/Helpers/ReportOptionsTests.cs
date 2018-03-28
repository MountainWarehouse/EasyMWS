using System.Linq;
using MountainWarehouse.EasyMWS.Helpers;
using NUnit.Framework;

namespace EasyMWS.Tests.Factories.Helpers
{
    public class ReportOptionsTests
    {
	    private ReportOptions _reportOptions;

	    [SetUp]
	    public void SetUp()
	    {
			_reportOptions = new ReportOptions();
		}

	    [Test]
	    public void AddStringOption_AddsTuplesToOptionsProperty_WithCorrectValues()
	    {
		    var initialNumberOfOptions = _reportOptions.Options.Count;
			_reportOptions.AddStringOption("testOptionName", "testOptionValue");
		    _reportOptions.AddStringOption("testOptionName2", "testOptionValue2");

			Assert.AreEqual(0, initialNumberOfOptions);
			Assert.AreEqual(2, _reportOptions.Options.Count);
			Assert.AreEqual("testOptionName", _reportOptions.Options.First().Name);
		    Assert.AreEqual("testOptionValue", _reportOptions.Options.First().Value);
		    Assert.IsNotNull(_reportOptions.Options.FirstOrDefault(t => t.Name == "testOptionName2"));
		}

	    [Test]
	    public void AddBooleanOption_AddsNewTupleToOptionsProperty_WithCorrectValues()
	    {
		    var initialNumberOfOptions = _reportOptions.Options.Count;
		    _reportOptions.AddBooleanOption("testOptionName", true);
		    _reportOptions.AddBooleanOption("testOptionName2", false);

			Assert.AreEqual(0, initialNumberOfOptions);
		    Assert.AreEqual(2, _reportOptions.Options.Count);
		    Assert.AreEqual("testOptionName", _reportOptions.Options.First().Name);
		    Assert.AreEqual("true", _reportOptions.Options.First().Value);
		    Assert.IsNotNull(_reportOptions.Options.FirstOrDefault(t => t.Name == "testOptionName2"));
		}

	    [Test]
	    public void AddIntegerOption_AddsNewTupleToOptionsProperty_WithCorrectValues()
	    {
		    var initialNumberOfOptions = _reportOptions.Options.Count;
		    _reportOptions.AddIntegerOption("testOptionName", 11);
		    _reportOptions.AddIntegerOption("testOptionName2", 22);

		    Assert.AreEqual(0, initialNumberOfOptions);
		    Assert.AreEqual(2, _reportOptions.Options.Count);
		    Assert.AreEqual("testOptionName", _reportOptions.Options.First().Name);
		    Assert.AreEqual("11", _reportOptions.Options.First().Value);
		    Assert.IsNotNull(_reportOptions.Options.FirstOrDefault(t => t.Name == "testOptionName2"));
		}

		[Test]
	    public void AddingMultipleOptionsOfDifferentTypes_AddsCorrectValuesToOptionsProperty()
	    {
			var initialNumberOfOptions = _reportOptions.Options.Count;
		    _reportOptions.AddStringOption("testOptionName1", "testOptionValue");
		    _reportOptions.AddBooleanOption("testOptionName2", true);
		    _reportOptions.AddIntegerOption("testOptionName3", 11);

		    Assert.AreEqual(0, initialNumberOfOptions);
		    Assert.AreEqual(3, _reportOptions.Options.Count);
			Assert.IsNotNull(_reportOptions.Options.FirstOrDefault(t => t.Name == "testOptionName1"));
			Assert.AreEqual("testOptionValue", _reportOptions.Options.First(t => t.Name == "testOptionName1").Value);
		    Assert.IsNotNull(_reportOptions.Options.FirstOrDefault(t => t.Name == "testOptionName2"));
		    Assert.AreEqual("true", _reportOptions.Options.First(t => t.Name == "testOptionName2").Value);
		    Assert.IsNotNull(_reportOptions.Options.FirstOrDefault(t => t.Name == "testOptionName3"));
		    Assert.AreEqual("11", _reportOptions.Options.First(t => t.Name == "testOptionName3").Value);
		}

		[Test]
	    public void GetOptionsString_ReturnsExpectedOptionsAsString()
	    {
			_reportOptions.AddStringOption("testOptionName1", "testOptionValue");
		    _reportOptions.AddBooleanOption("testOptionName2", true);
		    _reportOptions.AddIntegerOption("testOptionName3", 11);
		    var expectedOptionsAsString = "testOptionName1=testOptionValue;testOptionName2=true;testOptionName3=11;";

		    var actualOptionsAsString = _reportOptions.GetOptionsString();

			Assert.AreEqual(expectedOptionsAsString, actualOptionsAsString);
	    }
	}
}
