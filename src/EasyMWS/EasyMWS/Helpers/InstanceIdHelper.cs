using MountainWarehouse.EasyMWS.Model;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MountainWarehouse.EasyMWS.Helpers
{
    internal static class InstanceIdHelper
    {
        internal static string GetInstanceIdHash(EasyMwsOptions options)
        {
            string instanceId = null;
            try
            {
                instanceId = options?.CallbackInvocationOptions?.RestrictInvocationToOriginatingInstance?.CustomInstanceId ?? Environment.MachineName;
            }
            catch (InvalidOperationException)
            {
                instanceId = null;
            }
            return ComputeHash(instanceId);
        }

        private static string ComputeHash(string source)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(source));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
