using MGSC;
using QM_PathOfQuasimorph.Controllers;

namespace QM_PathOfQuasimorph.Processors
{
    internal class HelmetRecordProcessorPoq : ResistItemProcessor<HelmetRecord>
    {
        public HelmetRecordProcessorPoq(ItemRecordsControllerPoq controller) : base(controller) { }
    }
}