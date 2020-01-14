using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyMWS.Tests.Helpers
{
    public class EntryInvocationRestrictionHelperTests
    {
        private List<ReportRequestEntry> _reportRequestEntries;
        private const string _originatingInstance = "instanceId1";
        private string _originatingInstanceHash;
        private EasyMwsOptions _options;

        [SetUp]
        public void Setup()
        {
            _options = new EasyMwsOptions(useDefaultValues: true);
            _options.CallbackInvocationOptions.RestrictInvocationToOriginatingInstance.CustomInstanceId = _originatingInstance;
            _originatingInstanceHash = _options.CallbackInvocationOptions.RestrictInvocationToOriginatingInstance.HashedInstanceId;

            _reportRequestEntries = new List<ReportRequestEntry>
            {
                new ReportRequestEntry { Id = 1, InstanceId = _originatingInstanceHash, InvokeCallbackRetryCount = 0 },
                new ReportRequestEntry { Id = 2, InstanceId = "non-originating-instance1-hash", InvokeCallbackRetryCount = 3 },
                new ReportRequestEntry { Id = 3, InstanceId = "non-originating-instance2-hash", InvokeCallbackRetryCount = 0 },
                new ReportRequestEntry { Id = 4, InstanceId = _originatingInstanceHash, InvokeCallbackRetryCount = 0 },
            };
        }

        [Test]
        public void EntryInvocationRestrictionHelper_WithNonDefinedinvocationRestrictionOptions_ReturnsSameEntries()
        {
            _options.CallbackInvocationOptions.RestrictInvocationToOriginatingInstance = null;

            var filteredEntries = EntryInvocationRestrictionHelper<ReportRequestEntry>.RestrictInvocationToOriginatingClientsIfEnabled(_reportRequestEntries, _options);

            CollectionAssert.AreEquivalent(_reportRequestEntries, filteredEntries);
        }

        [Test]
        public void EntryInvocationRestrictionHelper_WithForceInvocationByOriginatingInstanceOptionSetToFalse_ReturnsSameEntries()
        {
            _options.CallbackInvocationOptions.RestrictInvocationToOriginatingInstance.ForceInvocationByOriginatingInstance = false;

            var filteredEntries = EntryInvocationRestrictionHelper<ReportRequestEntry>.RestrictInvocationToOriginatingClientsIfEnabled(_reportRequestEntries, _options);

            CollectionAssert.AreEquivalent(_reportRequestEntries, filteredEntries);
        }

        [TestCase(false, 2)]
        [TestCase(true, 3)]
        public void EntryInvocationRestrictionHelper_WithForceInvocationByOriginatingInstanceOptionSetToTrue_AndCustomInstanceId_ReturnsCorrectEntries(bool allowInvocationByAnyInstance, int expectedNumberOfEntries)
        {
            _options.CallbackInvocationOptions.RestrictInvocationToOriginatingInstance.ForceInvocationByOriginatingInstance = true;
            _options.CallbackInvocationOptions.RestrictInvocationToOriginatingInstance.AllowInvocationByAnyInstanceIfInvocationFailedLimitReached = allowInvocationByAnyInstance;
            var allowedFailures = 2;
            _options.CallbackInvocationOptions.RestrictInvocationToOriginatingInstance.InvocationFailuresLimit = allowedFailures;

            var filteredEntries = EntryInvocationRestrictionHelper<ReportRequestEntry>.RestrictInvocationToOriginatingClientsIfEnabled(_reportRequestEntries, _options);

            CollectionAssert.AreNotEquivalent(_reportRequestEntries, filteredEntries);
            Assert.IsTrue(filteredEntries.Count == expectedNumberOfEntries);

            if (!allowInvocationByAnyInstance) Assert.IsTrue(filteredEntries.All(e => e.InstanceId == _originatingInstanceHash));
            else Assert.IsTrue(
                filteredEntries.Count(e => e.InstanceId == _originatingInstanceHash) == 2 && 
                filteredEntries.Count(e => e.InstanceId != _originatingInstanceHash && e.InvokeCallbackRetryCount > allowedFailures) == 1);

        }

        [TestCase(false, 2)]
        [TestCase(true, 3)]
        public void EntryInvocationRestrictionHelper_WithForceInvocationByOriginatingInstanceOptionSetToTrue_AndDefaultInstanceId_ReturnsCorrectEntries(bool allowInvocationByAnyInstance, int expectedNumberOfEntries)
        {
            _options = new EasyMwsOptions(useDefaultValues: true);
            _options.CallbackInvocationOptions.RestrictInvocationToOriginatingInstance.CustomInstanceId = null;
            _originatingInstanceHash = _options.CallbackInvocationOptions.RestrictInvocationToOriginatingInstance.HashedInstanceId;

            _reportRequestEntries = new List<ReportRequestEntry>
            {
                new ReportRequestEntry { Id = 1, InstanceId = _originatingInstanceHash, InvokeCallbackRetryCount = 0 },
                new ReportRequestEntry { Id = 2, InstanceId = "non-originating-instance1-hash", InvokeCallbackRetryCount = 3 },
                new ReportRequestEntry { Id = 3, InstanceId = "non-originating-instance2-hash", InvokeCallbackRetryCount = 0 },
                new ReportRequestEntry { Id = 4, InstanceId = _originatingInstanceHash, InvokeCallbackRetryCount = 0 },
            };

            _options.CallbackInvocationOptions.RestrictInvocationToOriginatingInstance.ForceInvocationByOriginatingInstance = true;
            _options.CallbackInvocationOptions.RestrictInvocationToOriginatingInstance.AllowInvocationByAnyInstanceIfInvocationFailedLimitReached = allowInvocationByAnyInstance;
            var allowedFailures = 2;
            _options.CallbackInvocationOptions.RestrictInvocationToOriginatingInstance.InvocationFailuresLimit = allowedFailures;

            var filteredEntries = EntryInvocationRestrictionHelper<ReportRequestEntry>.RestrictInvocationToOriginatingClientsIfEnabled(_reportRequestEntries, _options);

            CollectionAssert.AreNotEquivalent(_reportRequestEntries, filteredEntries);
            Assert.IsTrue(filteredEntries.Count == expectedNumberOfEntries);

            if (!allowInvocationByAnyInstance) Assert.IsTrue(filteredEntries.All(e => e.InstanceId == _originatingInstanceHash));
            else Assert.IsTrue(
                filteredEntries.Count(e => e.InstanceId == _originatingInstanceHash) == 2 &&
                filteredEntries.Count(e => e.InstanceId != _originatingInstanceHash && e.InvokeCallbackRetryCount > allowedFailures) == 1);

        }
    }
}
