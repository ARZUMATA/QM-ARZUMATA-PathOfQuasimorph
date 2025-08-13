using MGSC;
using QM_PathOfQuasimorph.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.Records
{
    // We will use repair item approach as it's viable and suits our needs.
    public class MetaDataRecord : BasePickupItemRecord
    {
        public ItemRarity Rarity { get; set; }

        public MetaDataRecord()
        {
        }
    }
}
