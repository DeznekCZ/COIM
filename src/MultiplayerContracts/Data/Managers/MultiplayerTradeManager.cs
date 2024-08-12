using Mafi;
using Mafi.Core.Prototypes;
using System;
using System.IO;
using System.Text.RegularExpressions;
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

        public static Task<ContractId> CreateContract(ContractParameters contractParameters)
        {
            try
            {
                ContractId contractId = new ContractId(DateTime.UtcNow.Ticks, userId);

                Directory.CreateDirectory("contracts");
                var stream = File.CreateText($"contracts/{contractId.UserIdentifier}.{contractId.CreationTime}.open");
                stream.Write(contractParameters.ToJSON());
                stream.Close();

                return Task.FromResult(contractId);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return Task.FromResult<ContractId>(null);
            }
        }

        public static Task<bool> ClaimContract(ContractId contractId)
        {
            try
            {
                File.Delete($"contracts/{contractId.UserIdentifier}.{contractId.CreationTime}.used");
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return Task.FromResult(false);
            }
        }

        public static Task<bool> RemoveContract(ContractId contractId)
        {
            try
            {
                File.Delete($"contracts/{contractId.UserIdentifier}.{contractId.CreationTime}.open");
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// TODO refresh only selected tab, or on tab selected action
        /// </summary>
        /// <returns></returns>
        public static Task<ContractLists> GetContracts()
        {
            return Task.Run(() =>
            {
                ContractLists contractLists = new ContractLists();

                Regex statusRegex = new Regex(@"(?<user>\d+)\.(?<creation>\d+)\.(?<status>\w+)");

                if (Directory.Exists("contracts"))
                {
                    foreach (var file in Directory.EnumerateFiles("contracts"))
                    {
                        Log.Debug($"Contract found: {file}");
                        var text = File.OpenText(file);
                        var str = text.ReadToEnd();
                        text.Close();

                        Match statusData = statusRegex.Match(file);

                        string status = statusData.Groups["status"].Value;
                        long user = long.Parse(statusData.Groups["user"].Value);
                        long creation = long.Parse(statusData.Groups["creation"].Value);

                        var id = new ContractId(creation, user);

                        ContractParameters contractParameters;
                        try
                        {
                            contractParameters = ContractParameters.FromJSON(str, m_protosDb);
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                            continue;
                        }

                        if (user == userId)
                        {
                            contractLists.Owned[id] = contractParameters;

                            if (status == "used")
                            {
                                contractLists.Claimable.Add(id);
                            }
                        }
                        else
                        {
                            if (status == "open")
                            {
                                contractLists.Available[id] = ContractParameters.FromJSON(str, m_protosDb);
                            }
                        }
                    }
                }

                return contractLists;
            }).ContinueWith(task => {
                if (task.IsFaulted)
                {
                    Log.Exception(task.Exception);
                    return new ContractLists();
                }
                return task.Result;
            });
        }

        public static Task<bool> TakeContract(ContractId contractId)
        {
            return Task.Run(() =>
            {
                if (contractId.UserIdentifier == userId)
                    return false;

                if (!File.Exists($"contracts/{contractId.UserIdentifier}.{contractId.CreationTime}.open"))
                    return false;

                try
                {
                    string text = File.ReadAllText($"contracts/{contractId.UserIdentifier}.{contractId.CreationTime}.open");
                    File.Delete($"contracts/{contractId.UserIdentifier}.{contractId.CreationTime}.open");
                    File.WriteAllText($"contracts/{contractId.UserIdentifier}.{contractId.CreationTime}.used", text);
                    return true;
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                    return false;
                }
            }).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Log.Exception(task.Exception);
                    return false;
                }
                return task.Result;
            });
        }
    }
}
