using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using BcWin.Contract;
using log4net;

namespace BcManagement
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class BcManagementService : IBcManageService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BcManagementService));
        public int Ping()
        {
            return 1;
        }

        public int PingBet(string username, string mac, int key)
        {
            if (ProcessData.ClientPingChecks.ContainsKey(username))
            {
                if (ProcessData.ClientPingChecks[username] == mac)
                {
                    return 1;
                }
                else
                {
                    if (key == 10)
                    {
                        ProcessData.ClientPingChecks[username] = mac;
                        return 1;
                    }

                    return 0;
                }
            }
            else
            {
                ProcessData.ClientPingChecks[username] = mac;
                return 1;
            }
        }

        public long DateTimeToUnixTimestamp(DateTime dateTime)
        {

            return (long)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
        }

        public int PingScan(string ipAddress, int sboOnline, int ibetOnline)
        {
            return 1;
        }

        public bool Login(string username, string password, eUserType userType, string ip, string mac, string hostname)
        {
            try
            {
                using (var context = new WinDbEntities())
                {
                    var i = context.Users.FirstOrDefault(u => u.Username == username && u.Password == password
                        && u.Type == (byte)userType);
                    if (i == null)
                        return false;

                    i.LoginTraces.Add(new LoginTrace()
                    {
                        Hostname = hostname,
                        IpAddress = ip,
                        MacAddress = mac,
                        TimeLogin = DateTime.Now
                    });
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public int RouteType(string username)
        {
            try
            {
                using (var context = new WinDbEntities())
                {
                    return context.Users.FirstOrDefault(u => u.Username == username).RouteType.Value;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return int.MaxValue;
            }
        }

        public int GetAccType(string username)
        {
            try
            {
                using (var context = new WinDbEntities())
                {
                    return context.Users.FirstOrDefault(u => u.Username == username).AccType.Value;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return int.MaxValue;
            }
        }


        public SetupScanConfigDTO ScanConfig(string username, int type)
        {
            return ProcessData.ScanConfigs[type];
        }

        public ScanAccountDTO ScanAccount(string username)
        {
            try
            {
                using (var context = new WinDbEntities())
                {
                    var i = context.Users.FirstOrDefault(u => u.Username == username);
                    if (i == null)
                        throw new Exception("INVALID USERNAME");

                    var scanaccDto = new ScanAccountDTO();
                    scanaccDto.IbetAccounts = i.AccScanInUses.Where(a => a.ScanAccount.ServerType == 1).Select(
                        ac =>
                            new ScanAccountInfoDTO
                            {
                                Username = ac.ScanAccount.Username,
                                Password = ac.ScanAccount.Password
                            }
                        ).ToList();

                    scanaccDto.SboAccounts = i.AccScanInUses.Where(a => a.ScanAccount.ServerType == 2).Select(
                       ac =>
                           new ScanAccountInfoDTO
                           {
                               Username = ac.ScanAccount.Username,
                               Password = ac.ScanAccount.Password
                           }
                       ).ToList();

                    return scanaccDto;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ScanAccountDTO();
            }
        }

        public SetupBetConfigDTO BetSetupConfig(string username, int type)
        {
            try
            {
                SetupBetConfigDTO clientConfig = ProcessData.BetClientConfigs[type];

                using (var context = new WinDbEntities())
                {
                    var i = context.Users.FirstOrDefault(u => u.Username == username);
                    if (i == null)
                        throw new Exception("INVALID USERNAME");

                    clientConfig.SboScanAccounts = i.AccScanInUses.Where(a => a.ScanAccount.ServerType == 2).Select(
                       ac =>
                           new ScanAccountInfoDTO
                           {
                               Username = ac.ScanAccount.Username,
                               Password = ac.ScanAccount.Password
                           }
                       ).ToList();
                }

                return clientConfig;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return null;
        }

        public ScanInfoDTO ClientScanAccount(string username)
        {
            try
            {
                using (var context = new WinDbEntities())
                {
                    var i = context.Users.FirstOrDefault(u => u.Username == username);
                    if (i == null)
                        throw new Exception("INVALID USERNAME");

                    var scanaccDto = new ScanInfoDTO();
                    scanaccDto.ScanServers = context.SystemConfigs.Where(a => a.KeyConfig == "SBO_SCAN_LINK")
                      .Select(a => a.ValueConfig).ToList();
                    scanaccDto.Accounts = i.AccScanInUses.Where(a => a.ScanAccount.ServerType == 2).Select(
                       ac =>
                           new ScanAccountInfoDTO
                           {
                               Username = ac.ScanAccount.Username,
                               Password = ac.ScanAccount.Password
                           }
                       ).ToList();

                    return scanaccDto;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ScanInfoDTO();
            }
        }

        public ScanInfoDTO ClientScanAccountBuyServerType(string username, eServerType serverType)
        {
            try
            {
                int accType;
                string linkType;

                switch (serverType)
                {
                    case eServerType.Sbo:
                        accType = 2;
                        linkType = "SBO_SCAN_LINK";
                        break;
                    case eServerType.Ibet:
                        accType = 1;
                        linkType = "IBET_SCAN_LINK";
                        break;
                    default:
                        accType = 0;
                        linkType = "";
                        break;
                }

                using (var context = new WinDbEntities())
                {
                    var i = context.Users.FirstOrDefault(u => u.Username == username);
                    if (i == null)
                        throw new Exception("INVALID USERNAME");

                    var scanaccDto = new ScanInfoDTO();
                    scanaccDto.ScanServers = context.SystemConfigs.Where(a => a.KeyConfig == linkType)
                      .Select(a => a.ValueConfig).ToList();
                    scanaccDto.Accounts = i.AccScanInUses.Where(a => a.ScanAccount.ServerType == accType).Select(
                       ac =>
                           new ScanAccountInfoDTO
                           {
                               Username = ac.ScanAccount.Username,
                               Password = ac.ScanAccount.Password
                           }
                       ).ToList();

                    return scanaccDto;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ScanInfoDTO();
            }
        }

        public string GetKey()
        {
            try
            {
                using (var context = new WinDbEntities())
                {
                    return context.SystemConfigs.FirstOrDefault(u => u.KeyConfig == "KEY_VALID").ValueConfig;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return string.Empty;
            }
        }
    }
}
