using System.Linq;
using MountainWarehouse.EasyMWS.Data;
using NUnit.Framework;

namespace EasyMWS.Tests.Data
{
	[TestFixture]
    public class DbContextTests
    {
	    private EasyMwsContext _dbContext;
	    private string _testTypeName = "test report type #123";

	    [SetUp]
	    public void Setup()
	    {
			_dbContext = new EasyMwsContext();
		    _dbContext.ReportRequestCallbacks.Add(new ReportRequestCallback {TypeName = _testTypeName });
		    _dbContext.SaveChanges();
	    }

	    [TearDown]
	    public void TearDown()
	    {
		    var testReport = _dbContext.ReportRequestCallbacks.First(r => r.TypeName == _testTypeName);
		    _dbContext.ReportRequestCallbacks.Remove(testReport);
		    _dbContext.SaveChanges();
	    }

	    [Test]
	    public void OnWhere_ReturnsAnyData()
	    {
		    var testId = _dbContext.ReportRequestCallbacks.FirstOrDefault(r => r.TypeName == _testTypeName);

			Assert.NotNull(testId);
	    }
    }
}
