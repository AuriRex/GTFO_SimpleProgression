using BepInEx.Logging;
using SimpleProgression.Interfaces;
using System;

namespace SimpleProgression
{
    public class Logger : ILogger
    {
        private readonly ManualLogSource _log;

        public Logger(ManualLogSource logger)
        {
            _log = logger;
        }

        public void Debug(string msg)
        {
            _log.LogDebug(msg);
        }

        public void Error(string msg)
        {
            _log.LogError(msg);
        }

        public void Exception(Exception ex)
        {
            _log.LogError($"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }

        public void Fail(string msg)
        {
            _log.LogError(msg);
        }

        public void Info(string msg)
        {
            _log.LogMessage(msg);
        }

        public void Msg(ConsoleColor col, string msg)
        {
            _log.LogMessage(msg);
        }

        public void Notice(string msg)
        {
            _log.LogWarning(msg);
        }

        public void Success(string msg)
        {
            _log.LogMessage(msg);
        }

        public void Warning(string msg)
        {
            _log.LogWarning(msg);
        }
    }
}
