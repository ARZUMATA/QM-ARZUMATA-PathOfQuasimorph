using MGSC;
using QM_PathOfQuasimorph.Controllers;

namespace QM_PathOfQuasimorph.Processors
{
    internal class LeggingsRecordProcessorPoq : ResistItemProcessor<LeggingsRecord>
    {
        public LeggingsRecordProcessorPoq(ItemRecordsControllerPoq controller) : base(controller) { }
    }
}