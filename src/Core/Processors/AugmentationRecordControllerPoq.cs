using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.Core.Processors
{
    internal class AugmentationRecordControllerPoq
    {
        private ItemRecordsControllerPoq itemRecordsControllerPoq;

        public AugmentationRecordControllerPoq(ItemRecordsControllerPoq itemRecordsControllerPoq)
        {
            this.itemRecordsControllerPoq = itemRecordsControllerPoq;
        }
    }
}
