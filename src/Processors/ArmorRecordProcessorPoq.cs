using MGSC;
using QM_PathOfQuasimorph.Controllers;

namespace QM_PathOfQuasimorph.Processors
{
    internal class ArmorRecordProcessorPoq : ResistItemProcessor<ArmorRecord>
    {
        public ArmorRecordProcessorPoq(ItemRecordsControllerPoq controller) : base(controller) { }
    }
}