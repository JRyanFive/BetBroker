using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace BcWin.Core.Helper
{
    public class EngineLogger
    {
        private ILog logger;
        public eServerType ServerType { get; set; }

        public EngineLogger(string name)
        {
            RollingFileAppender rollingFileAppender = new RollingFileAppender();
            rollingFileAppender.File = string.Format("logs/{0}.dat", name);
            rollingFileAppender.AppendToFile = true;
            rollingFileAppender.MaxSizeRollBackups = 20;
            rollingFileAppender.MaximumFileSize = "1000KB";
            ILayout layout = new PatternLayout("%date [%thread] %message%newline");
            rollingFileAppender.Layout = layout;
            rollingFileAppender.Name = name;
            rollingFileAppender.ActivateOptions();
            Hierarchy hierarchy = LogManager.GetRepository() as Hierarchy;
            Logger logger = hierarchy.GetLogger(name) as Logger;
            logger.AddAppender(rollingFileAppender);
            this.logger = LogManager.GetLogger(name);
        }

        public void Log(string message)
        {
            string message2 = AppendTraces(message);
            this.logger.Info(message2);
        }

        public void Error(string message)
        {
            string message2 = AppendTraces("[ERROR] " + message);
            this.logger.Error(message2);
        }

        public void Error(string message, Exception ex)
        {
            string message2 = AppendTraces("[ERROR] " + message);
            this.logger.Error(message2, ex);
        }

        public void Error(Exception ex)
        {
            string message2 = AppendTraces("[ERROR] ");
            this.logger.Error(message2, ex);
        }

        private string AppendTraces(string message)
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame[] frames = stackTrace.GetFrames();
            int num = (frames.Length > 4) ? 4 : frames.Length;
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(ServerType + " : ");
            for (int i = 2; i < num; i++)
            {
                stringBuilder.Append(string.Format("{0}->", frames[i].GetMethod().Name));
            }
            stringBuilder.Append(string.Format(": {0}", message));
            return stringBuilder.ToString();
        }
    }
}
