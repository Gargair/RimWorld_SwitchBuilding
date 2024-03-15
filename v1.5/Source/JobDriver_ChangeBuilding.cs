using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;

namespace SwitchBuilding
{
    public class JobDriver_ChangeBuilding : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield break;
        }
    }
}
