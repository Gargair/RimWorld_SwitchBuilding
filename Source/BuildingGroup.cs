using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace UpgradeBuildings
{
    internal class BuildingGroup : DefModExtension
    {
#pragma warning disable 0649
        public string buildingGroup;
#pragma warning restore 0649

        public BuildingGroup() { }

        public override IEnumerable<string> ConfigErrors()
        {
            if (string.IsNullOrWhiteSpace(buildingGroup))
            {
                yield return "UpgBldg.NoBuildingGroup".Translate();
            }
            yield break;
        }
    }
}
