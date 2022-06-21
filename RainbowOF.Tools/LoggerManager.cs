using NLog;
using System;

namespace RainbowOF.Tools
{
    public class LoggerManager : ILoggerManager
    {
        private static ILogger appLoggerManager = LogManager.GetCurrentClassLogger();
        #region Init
        public LoggerManager()
        {
        }
        #endregion
        #region Support Routines
        private string CleanMessage(string rawMessage)
        {
            rawMessage = rawMessage.Replace(Environment.NewLine + Environment.NewLine, "; ");
            rawMessage = rawMessage.Replace(Environment.NewLine, ";");
            return rawMessage;
        }
        #endregion
        #region Interface routines
        public void LogDebug(string message)
        {
            appLoggerManager.Debug(CleanMessage(message));
        }
        public void LogError(string message)
        {
            appLoggerManager.Error(CleanMessage(message));
        }
        public void LogInfo(string message)
        {
            appLoggerManager.Info(CleanMessage(message));
        }
        public void LogWarn(string message)
        {
            appLoggerManager.Warn(CleanMessage(message));
        }
        public bool IsDebugEnabled()
        {
            return appLoggerManager.IsDebugEnabled;
        }
        #endregion
    }
}
