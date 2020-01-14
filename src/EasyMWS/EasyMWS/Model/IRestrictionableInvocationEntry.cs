namespace MountainWarehouse.EasyMWS.Model
{
    public interface IRestrictionableInvocationEntry
    {
        string InstanceId { get; set; }
        int InvokeCallbackRetryCount { get; set; }
    }
}
