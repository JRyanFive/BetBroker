using System;
using System.Collections.Generic;
using System.ServiceModel;
using BcWin.Common.Contract;
using BcWin.Common.DTO;
using BcWin.Common.FaultDTO;
using BcWin.Processor;

namespace BcWin.Server.Service
{
    public class BcService : IBcService
    {
        public int Ping()
        {
            return 1;
        }

        public Guid LogIn(string userName, string password)
        {
            throw new NotImplementedException();
        }

        public IList<string> GetSboServers()
        {
            return DataContainer.SbobetServers;
        }

        public IList<string> GetIbetServers()
        {
            return DataContainer.IbetServers;
        }

        //Goi ham check ten mien, if has gui => ok
        public Guid? InitBetServer(string url)
        {
            Guid? gId = Guid.Empty;
            var frmService = FrmService.CurrentInstance;
            frmService.MySynchronizationContext.Send(_ => frmService.CreateNewTab(url, out gId), null);
            return gId;
        }
        
        //Goi start cap sbo-ibet, doi 10s, 
        public Guid InitServer(Guid guidToken, AccountDTO firstAccountDto, AccountDTO secondAccountDto,
             ProcessorConfigInfoDTO processorConfigDto)
        {
            Guid procesessGuid = Guid.NewGuid();
            var frmService = FrmService.CurrentInstance;
            frmService.MySynchronizationContext.Send(_ =>
                frmService.InitProcessor(procesessGuid, firstAccountDto, secondAccountDto), null);
            return procesessGuid;
        }

        //Sleep 10s, call this 
        public void StartServer(Guid processorToken)
        {
            var frmService = FrmService.CurrentInstance;
            frmService.MySynchronizationContext.Send(_ =>
                frmService.StartProcessor(processorToken), null);
        }

        public ProcessorConfigInfoDTO GetServerConfigInfo(Guid tokenServer)
        {
            throw new NotImplementedException();
        }

        public void StopServer(Guid tokenServer)
        {
            Guid gId = Guid.NewGuid();
            var frmService = FrmService.CurrentInstance;
            frmService.MySynchronizationContext.Send(_ =>
                frmService.StopProcessor(tokenServer), null);
        }

        public string GetServerReport(Guid processGuid, AccountDTO accountDto)
        {
            var frmService = FrmService.CurrentInstance;
            string report = string.Empty;
            //frmService.MySynchronizationContext.Send(_ =>
            //{
            //    report = frmService.GetBetReport(processGuid, accountDto);
            //}, null);
            return report;
        }

        public string GetServiceReport(Guid tokenServer)
        {
            throw new NotImplementedException();
        }
    }
}
