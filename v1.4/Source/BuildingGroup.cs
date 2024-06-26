﻿using System.Collections.Generic;
using Verse;

namespace SwitchBuilding
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
                yield return "SwitchBldg.NoBuildingGroup".Translate();
            }
            yield break;
        }
    }
}
