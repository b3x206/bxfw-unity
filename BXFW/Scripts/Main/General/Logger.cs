using System;
using System.Reflection;

namespace BXFW
{
    /// <summary>
    /// Defines a basic generic logger, where the purpose is just to print stuff.
    /// <br>This is not a substitute for a normal logger, you have to attach a proper logger here.</br>
    /// </summary>
    public sealed class Logger
    {
        public delegate void LogAction(object message);
        public delegate void LogExceptionAction(Exception exception);

        private readonly LogAction logStandardAction;
        private readonly LogAction logWarningAction;
        private readonly LogAction logErrorAction;
        private readonly LogExceptionAction logExceptionAction;

        /// <summary>
        /// Creates a logger. The <paramref name="logAction"/> cannot be left null.
        /// </summary>
        public Logger(LogAction logAction, LogAction warnAction, LogAction errorAction, LogExceptionAction logException)
        {
            if (logAction == null)
            {
                throw new ArgumentNullException(nameof(logAction), "[Logger::(ctor)Logger] Given parameter was null.");
            }

            logStandardAction = logAction;
            logWarningAction = warnAction;
            logErrorAction = errorAction;
            logExceptionAction = logException;
        }

        /// <summary>
        /// Prints out a standard log.
        /// </summary>
        public void Log(object message)
        {
            logStandardAction(message);
        }
        /// <summary>
        /// Prints out a warning priority log.
        /// </summary>
        public void LogWarning(object message)
        {
            if (logWarningAction == null)
            {
                Log($"[Logger::LogWarning] There is no 'logWarningAction' available. Message : {message}");
                return;
            }

            logWarningAction(message);
        }
        /// <summary>
        /// Prints out an error priority log.
        /// </summary>
        public void LogError(object message)
        {
            if (logErrorAction == null)
            {
                Log($"[Logger::LogError] There is no 'logErrorAction' available. Message : {message}");
                return;
            }

            logErrorAction(message);
        }
        /// <summary>
        /// Prints out an exception.
        /// <br>A special unity method, this will use <see cref="LogError"/> if there's no <see cref="logExceptionAction"/> assigned.</br>
        /// </summary>
        public void LogException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception), "[Logger::LogException] Given parameter is null.");
            }

            if (logExceptionAction == null)
            {
                LogError($"[Logger::LogException] There is no 'logExceptionAction' available. Message : {exception.Message}, {exception.StackTrace}");
                return;
            }

            logExceptionAction(exception);
        }
        /// <summary>
        /// Prints out an exception.
        /// <br>A special unity method, this will use <see cref="LogError"/> if there's no <see cref="logExceptionAction"/> assigned.</br>
        /// </summary>
        public void LogException(string exceptionPrependMessage, Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception), "[Logger::LogException] Given parameter is null.");
            }

            if (!string.IsNullOrEmpty(exceptionPrependMessage))
            {
                // Strings are immutable
                string exMessage = exception.Message;
                // Prepend the given string to 'exception._message' internal value
                FieldInfo fiMessage = exception.GetType().GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fiMessage != null)
                {
                    fiMessage.SetValue(exception, exMessage.Insert(0, exceptionPrependMessage));
                }
                else
                {
                    LogError($"[Logger::LogException] There's no such field as '_message' inside exception '{exception}'.");
                }
            }

            LogException(exception);
        }
    }
}
