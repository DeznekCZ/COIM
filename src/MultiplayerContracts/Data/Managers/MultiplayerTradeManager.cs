using Mafi;
using Mafi.Core;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MultiplayerContracts
{
    //[GlobalDependency(RegistrationMode.AsEverything, false, false)]
    public class MultiplayerTradeManager
    {
        private static long userId = 54545642434564L;
        private static ProtosDb m_protosDb;

        public static void Init(ProtosDb protosDb)
        {
            m_protosDb = protosDb;
        }

        public static Task<bool> CreateContract(string address, string authorization, ContractParameters contractParameters)
        {
            return Task.Run(() =>
            {
                Log.Debug($"http://{address}/coi/market/quicktrade/create");

                WebRequest webRequest = WebRequest
                    .CreateHttp($"http://{address}/coi/market/quicktrade/create"); 

                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";
                webRequest.Headers.Add("Authorization", authorization);
                webRequest.Timeout = 3000;

                byte[] bytes = Encoding.UTF8.GetBytes(contractParameters.ToJSON());

                webRequest.ContentLength = bytes.Length;

                using (var stream = webRequest.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                WebResponse webResponse = webRequest.GetResponse();
                using(var stream = webResponse.GetResponseStream())
                {
                    byte[] newBytes = new byte[webResponse.ContentLength];
                    stream.Read(newBytes, 0, newBytes.Length);

                    return long.Parse(Encoding.UTF8.GetString(newBytes));
                }
            }).ContinueWith(t => {
                return t.IsCompleted;
            });
        }

        /// <summary>
        /// TODO refresh only selected tab, or on tab selected action
        /// </summary>
        /// <returns></returns>
        public static Task<ContractLists> GetContracts(string address, string authorization)
        {
            return Get($"http://{address}/coi/market/quicktrade/list", authorization, 10000)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Log.Exception(t.Exception);
                        return new ContractLists();
                    }
                    else if (t.Result.Item1 != HttpStatusCode.OK)
                    {
                        Log.Error($"Request failed: {t.Result.Item1}");
                        return new ContractLists();
                    }
                    else {
                        return ContractLists.ParseJSON(t.Result.Item2, m_protosDb);
                    }
                });
        }

        public static Task<(bool, string)> Ping(string address)
        {
            return Get($"http://{address}/coi/market/ping", "", 1000)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Log.Exception(t.Exception);
                        return (false, null);
                    }
                    else
                        return (t.Result.Item1 == HttpStatusCode.OK, t.Result.Item1 == HttpStatusCode.OK ? t.Result.Item2 : null);
                });
        }

        public static Task<string> Register(string address, string authorization, EntityId entityId)
        {
            return PostJSON($"http://{address}/coi/market/register", authorization)
                .ContinueWith(t => {
                    if (t.IsCompleted)
                    {
                        if (t.Result.Item1 == HttpStatusCode.OK)
                            return $@"{{""EntityId"":{entityId.Value},""CreationTime"":{t.Result.Item2}}}";
                        else
                            return authorization;
                    }
                    else
                    {
                        Log.Exception(t.Exception);
                        return authorization;
                    }
                });
        }

        private static Task<(HttpStatusCode, string)> PostJSON(string request, string body, string authorization = null, int timeout = 3000)
        {
            return Task.Run(() =>
            {
                try
                {
                    HttpWebRequest webRequest = WebRequest.CreateHttp(request);
                    Log.Debug(request + " init");

                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/json";
                    webRequest.Timeout = timeout;
                    webRequest.ReadWriteTimeout = timeout;

                    byte[] bytes = Encoding.UTF8.GetBytes(body);

                    webRequest.ContentLength = bytes.Length;

                    Log.Debug(request + " created");
                    using (var stream = webRequest.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    Log.Debug(request + " writen");

                    HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                    Log.Debug(request + " recived");
                    using (var stream = webResponse.GetResponseStream())
                    {
                        byte[] newBytes = new byte[webResponse.ContentLength];
                        int len = stream.Read(newBytes, 0, newBytes.Length);

                        Log.Debug(request + " read");
                        return (webResponse.StatusCode, Encoding.UTF8.GetString(newBytes, 0, len));
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                    return (HttpStatusCode.RequestTimeout, "");
                }
            });
        }

        private static Task<(HttpStatusCode, string)> Get(string request, string authorization = null, int timeout = 3000)
        {
            return Task.Run(() =>
            {
                try
                {
                    Log.Debug(request + " init");
                    HttpWebRequest webRequest = WebRequest.CreateHttp(request);

                    webRequest.Method = "GET";
                    webRequest.Timeout = timeout;
                    webRequest.ReadWriteTimeout = timeout;
                    if (authorization != null)
                        webRequest.Headers.Add("Authorization", authorization);

                    Log.Debug(request + " created");
                    HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                    Log.Debug(request + " recived");
                    using (var stream = webResponse.GetResponseStream())
                    {
                        byte[] newBytes = new byte[webResponse.ContentLength];
                        int len = stream.Read(newBytes, 0, newBytes.Length);

                        Log.Debug(request + " read");
                        return (webResponse.StatusCode, Encoding.UTF8.GetString(newBytes, 0, len));
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                    return (HttpStatusCode.RequestTimeout, "");
                }
            });
        }

        public static Task<bool> TakeContract(string address, string authorization, long contractId)
        {
            return Get($"http://{address}/coi/market/quicktrade/take/{contractId}", authorization)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Log.Exception(t.Exception);
                        return false;
                    }
                    else if (t.Result.Item1 != HttpStatusCode.OK)
                    {
                        Log.Error($"Request failed: {t.Result.Item1}");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                });
        }

        public static Task<bool> ClaimContract(string address, string authorization, long contractId)
        {
            return Get($"http://{address}/coi/market/quicktrade/claim/{contractId}", authorization)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Log.Exception(t.Exception);
                        return false;
                    }
                    else if (t.Result.Item1 != HttpStatusCode.OK)
                    {
                        Log.Error($"Request failed: {t.Result.Item1}");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                });
        }

        public static Task<bool> RemoveContract(string address, string authorization, long contractId)
        {
            return Get($"http://{address}/coi/market/quicktrade/remove/{contractId}", authorization)
                   .ContinueWith(t =>
                   {
                       if (t.IsFaulted)
                       {
                           Log.Exception(t.Exception);
                           return false;
                       }
                       else if (t.Result.Item1 != HttpStatusCode.OK)
                       {
                           Log.Error($"Request failed: {t.Result.Item1}");
                           return false;
                       }
                       else
                       {
                           return true;
                       }
                   });
        }
    }
}
