using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.Core.Records
{
    // We will use repair item approach as it's viable and suits our needs.
    public class AmplifierRecord: RepairRecord, IStackableRecord, IAllowInVestRecord
    {
        public override int InventorySortOrder
        {
            get
            {
                return 40;
            }
        }

        public ItemRarity Rarity { get; set; }

        public AmplifierRecord()
        {
        }
    }
}
