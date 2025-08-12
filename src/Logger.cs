using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QM_PathOfQuasimorph
{
    /// <summary>
    /// A Unity Debug Logger that includes the assembly name.
    /// Makes finding the mod that created the log entry easy to find.
    /// Calls the Unity.Debug functions that match the function names.
    /// </summary>
    public class Logger
    {
        public bool IsEnabled
        {
            get { return Plugin.Config?.DebugLog ?? false; }
            set { Plugin.Config.DebugLog = value; }
        }

        /// <summary>
        /// The identifier to include at the start of every line.
        /// If not set, defaults to this assembly's name.
        /// </summary>
        public string LogPrefix { get; set; }

        // List of types to exclude from logging
        public static HashSet<Type> _excludedTypes = new HashSet<Type>();

        // Cached calling type
        private readonly Type _callerType;

        public Logger(string logPrefix = "", Type callerType = null)
        {
            if (string.IsNullOrEmpty(logPrefix))
            {
                logPrefix = Assembly.GetExecutingAssembly().GetName().Name;
            }

            LogPrefix = logPrefix;
            _callerType = callerType;
        }

        public void ExcludeType<T>()
        {
            _excludedTypes.Add(typeof(T));
        }

        // Helper method to determine the calling type if it is null
        private Type GetCallerType()
        {
            if (_callerType != null)
            {
                return _callerType;
            }

            // Dynamically determine the caller type from the stack trace
            var stackTrace = new System.Diagnostics.StackTrace();
            var frames = stackTrace.GetFrames();

            // Start from the second frame (GetCallerType) and go deeper
            for (int i = 1; i < (frames?.Length ?? 0); i++)
            {
                var frame = frames[i];
                var method = frame.GetMethod();

                // If the method is not in the Logger class and is not a Unity method, it's the caller
                if (method.DeclaringType != typeof(Logger) && method.DeclaringType != typeof(Debug))
                {
                    return method.DeclaringType;
                }
            }

            return null;
        }

        public void Log(string message)
        {
            if (!IsEnabled) return;
            var callerType = GetCallerType();
            if (callerType != null && !_excludedTypes.Contains(callerType))
            {
                Debug.Log($"[{LogPrefix}] [{callerType.Name}] {message}");
            }
        }

        public void LogWarning(string message)
        {
            if (!IsEnabled) return;
            var callerType = GetCallerType();
            if (callerType != null && !_excludedTypes.Contains(callerType))
            {
                Debug.LogWarning($"[{LogPrefix}] [{callerType.Name}] [WARNING] {message}");
            }
        }

        public void LogError(string message)
        {
            if (!IsEnabled) return;
            var callerType = GetCallerType();
            if (callerType != null && !_excludedTypes.Contains(callerType))
            {
                Debug.LogError($"[{LogPrefix}] [{callerType.Name}] [ERROR] {message}");
            }
        }

        public void LogException(Exception ex)
        {
            if (!IsEnabled) return;
            var callerType = GetCallerType();
            if (callerType != null && !_excludedTypes.Contains(callerType))
            {
                Debug.LogError($"[{LogPrefix}] [{callerType.Name}] [EXCEPTION] Exception Logged:");
                Debug.LogException(ex);
            }
        }
    }
}
