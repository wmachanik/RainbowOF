using NLog;
using System;

namespace RainbowOF.Tools
{
    public class LoggerManager : ILoggerManager
    {
        private static ILogger _Logger = LogManager.GetCurrentClassLogger();
        private string CleanMessage(string rawMessage)
        {
            rawMessage = rawMessage.Replace(Environment.NewLine + Environment.NewLine, "; ");
            rawMessage = rawMessage.Replace(Environment.NewLine, ";");
            return rawMessage;
        }
        public void LogDebug(string message)
        {
            _Logger.Debug(CleanMessage(message));
        }
        public void LogError(string message)
        {
            _Logger.Error(CleanMessage(message));
        }
        public void LogInfo(string message)
        {
            _Logger.Info(CleanMessage(message));
        }
        public void LogWarn(string message)
        {
            _Logger.Warn(CleanMessage(message));
        }
    }
}
