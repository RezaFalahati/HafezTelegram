using System;
using System.Text;
using System.Threading.Tasks;
using TdLib;

namespace HafezTelegram.DataSource
{
    public static class DataReceiveHelper
    {
        private static readonly int ApiId = 92071;
        private static readonly string ApiHash = "a85a997adbdb3a6993ceb0d9a3f3ae58";

        public static StringBuilder Information = new StringBuilder();
        public static long LoopCount { get; set; }
        public static bool IsAuthorised { get; set; }

        public static async Task<TdClient> NewClientAsync()
        {
            try
            {
                var client = new TdClient();
                await client.ExecuteAsync(new TdApi.SetTdlibParameters
                {
                    Parameters = new TdApi.TdlibParameters
                    {
                        ApiId = ApiId,
                        ApiHash = ApiHash,
                        ApplicationVersion = "1.6.0",
                        DeviceModel = "VPS",
                        SystemLanguageCode = "en",
                        SystemVersion = "Win Server 2016"
                    }
                });
                await client.ExecuteAsync(new TdApi.CheckDatabaseEncryptionKey());
                return client;
            }
            catch (TdException e)
            {
                var error = e.Error;
                Information.AppendLine("NewClient-1" + error.Message);
            }
            catch (Exception ex)
            {
                Information.AppendLine("NewClient-2" + ex.Message);
            }

            return null;
        }
    }
}