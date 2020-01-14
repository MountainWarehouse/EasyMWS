using MountainWarehouse.EasyMWS.Model;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MountainWarehouse.EasyMWS.Helpers
{
    internal static class InstanceIdHelper
    {
        /// <summary>
        /// If customInstanceId is NullOrEmpty then the instanceId will be considered to be Environment.MachineName
        /// </summary>
        internal static string GetInstanceIdHash(string customInstanceId)
        {
            string instanceId = null;
            try
            {
                instanceId = string.IsNullOrEmpty(customInstanceId) ? Environment.MachineName : customInstanceId;
            }
            catch (InvalidOperationException)
            {
                instanceId = null;
            }
            return ComputeHash(instanceId);
        }

        private static string ComputeHash(string source)
        {
            if (string.IsNullOrEmpty(source)) return null;

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
