using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using BcWin.Common.Objects;
using BCWin.Engine.Ibet;
using BCWin.Engine.Sbo;

namespace BcWin.Processor
{
    public abstract class BaseProcessor
    {
        public eServiceStatus Status { get; set; }

        public IbetEngine IbetEngine { get; set; }

        public SboEngine SboEngine { get; set; }

        public ProcessorConfigInfoDTO ProcessorConfigInfo { get; set; }

        public eScanType ScanType { get; set; }

        protected System.Threading.Timer objUpdateCreditTimer { get; set; }
    }
}
