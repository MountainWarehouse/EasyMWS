using System.Linq;
using MountainWarehouse.EasyMWS.Data;
using NUnit.Framework;

namespace EasyMWS.Tests.Data
{
	[TestFixture]
    public class DbContextTests
    {
	    private EasyMwsContext _dbContext;
	    private string _testReportName = "test report name #123";

	    [SetUp]
	    public void Setup()
	    {
			_dbContext = new EasyMwsContext();
		    _dbContext.Reports.Add(new Report {Name = _testReportName });
		    _dbContext.SaveChanges();
	    }

	    [TearDown]
	    public void TearDown()
	    {
		    var testReport = _dbContext.Reports.First(r => r.Name == _testReportName);
		    _dbContext.Reports.Remove(testReport);
		    _dbContext.SaveChanges();
	    }

	    [Test]
	    public void OnWhere_ReturnsAnyData()
	    {
		    var testId = _dbContext.Reports.FirstOrDefault(r => r.Name == _testReportName);

			Assert.NotNull(testId);
	    }
    }
}
