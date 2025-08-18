using MGSC;
using QM_PathOfQuasimorph.Controllers;

namespace QM_PathOfQuasimorph.Processors
{
    internal class BootsRecordProcessorPoq : ResistItemProcessor<BootsRecord>
    {
        public BootsRecordProcessorPoq(ItemRecordsControllerPoq controller) : base(controller) { }
    }
}