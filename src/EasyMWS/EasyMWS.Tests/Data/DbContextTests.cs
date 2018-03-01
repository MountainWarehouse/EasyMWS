using System.Linq;
using MountainWarehouse.EasyMWS.Data;
using NUnit.Framework;

namespace EasyMWS.Tests.Data
{
	[TestFixture]
    public class DbContextTests
    {
	    private EasyMwsContext _dbContext;
	    private int _testReportId = 999999999;

	    [SetUp]
	    public void Setup()
	    {
			_dbContext = new EasyMwsContext();
		    _dbContext.Reports.Add(new Report {Id = _testReportId});
		    _dbContext.SaveChanges();
	    }

	    [TearDown]
	    public void TearDown()
	    {
		    var testReport = _dbContext.Reports.First(r => r.Id == _testReportId);
		    _dbContext.Reports.Remove(testReport);
		    _dbContext.SaveChanges();
	    }

	    [Test]
        [Ignore("Will only run if sql server is installed on current machine. Quite slow to fail.")]
	    public void OnWhere_ReturnsAnyData()
	    {
		    var testId = _dbContext.Reports.FirstOrDefault(r => r.Id == _testReportId);

			Assert.NotNull(testId);
	    }
    }
}
