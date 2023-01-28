using System.Linq;
using Verse;

namespace UpgradeBuildings
{
    [StaticConstructorOnStartup]
    public static class UpgradeBuildings
    {
        static UpgradeBuildings()
        {
            LogMessage(LogLevel.Information, "Welcome to pointless log spam");
            var changeComp = new CompProperties_ChangeBuilding();
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs
                        .Where(thingDef => thingDef.HasModExtension<BuildingGroup>())
                        .Where(thingDef => !thingDef.HasComp(typeof(CompProperties_ChangeBuilding))))
            {
                thingDef.comps.Add(changeComp);
            }
            LogMessage(LogLevel.Debug, "Finished adding comps to thingDefs");
        }

        public static LogLevel logLevel = LogLevel.Debug;

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
