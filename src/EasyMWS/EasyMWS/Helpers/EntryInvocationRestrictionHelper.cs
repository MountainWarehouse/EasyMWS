using MountainWarehouse.EasyMWS.Model;
using System.Collections.Generic;
using System.Linq;

namespace MountainWarehouse.EasyMWS.Helpers
{
    internal static class EntryInvocationRestrictionHelper<T> where T : IRestrictionableInvocationEntry
    {
        internal static List<T> RestrictInvocationToOriginatingClientsIfEnabled(IEnumerable<IRestrictionableInvocationEntry> entries, EasyMwsOptions options)
        {
            var invocationRestrictionOptions = options?.CallbackInvocationOptions?.RestrictInvocationToOriginatingInstance;
            if (invocationRestrictionOptions != null && invocationRestrictionOptions.ForceInvocationByOriginatingInstance == true)
            {
                var entriesEligibleForCallbackInvocation = entries.Where(e => e.InstanceId == invocationRestrictionOptions.HashedInstanceId).ToList();

                if (invocationRestrictionOptions.AllowInvocationByAnyInstanceIfInvocationFailedLimitReached)
                {
                    // allow any client to attempt callback invocation if the configured amount of invocation failures have happened

                    var entriesWithInvocationFailureLimitReached = entries.Where(e => 
                        e.InvokeCallbackRetryCount >= invocationRestrictionOptions.InvocationFailuresLimit && 
                        e.InstanceId != invocationRestrictionOptions.HashedInstanceId).ToList();
                    entriesEligibleForCallbackInvocation.AddRange(entriesWithInvocationFailureLimitReached);

                    return entriesEligibleForCallbackInvocation.Select(e => (T)e).ToList();
                }
                else
                {
                    // restrict callback invocation to clients matching the current hashedInstanceId
                    return entriesEligibleForCallbackInvocation.Select(e => (T)e).ToList();
                }
            }

            return entries.Select(e => (T)e).ToList();
        }
    }
}
