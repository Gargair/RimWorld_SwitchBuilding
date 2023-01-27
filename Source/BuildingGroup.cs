using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace UpgradeBuildings
{
    internal class BuildingGroup: DefModExtension
    {
        public string buildingGroup;

        public override IEnumerable<string> ConfigErrors()
        {
            if(string.IsNullOrWhiteSpace(buildingGroup))
            {
                yield return "UpgBldg.NoBuildingGroup".Translate();
            }
            yield break;
        }
    }
}
