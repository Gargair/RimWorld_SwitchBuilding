using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace UpgradeBuildings
{
    [StaticConstructorOnStartup]
    public static class UpgradeBuildings
    {
        static UpgradeBuildings()
        {

        }

        public static LogLevel logLevel = LogLevel.Warning;

        public static void LogMessage(LogLevel logLevel, params string[] messages)
        {
            var actualMessage = messages.Aggregate("[UpgradeBuildings]", (logMessage, message) => logMessage + " " + message);
            if (logLevel > UpgradeBuildings.logLevel)
            {
                return;
            }
            switch (logLevel)
            {
                case LogLevel.Error:
                    Log.Error(actualMessage);
                    break;
                case LogLevel.Warning:
                    Log.Warning(actualMessage);
                    break;
                default:
                    Log.Message(actualMessage);
                    break;
            }
        }
    }

    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Information = 3,
        Debug = 4,
    }
}
