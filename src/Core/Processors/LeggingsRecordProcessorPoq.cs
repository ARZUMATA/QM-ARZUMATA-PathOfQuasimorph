using MGSC;

namespace QM_PathOfQuasimorph.Core.Processors
{
    internal class LeggingsRecordProcessorPoq : ResistItemProcessor<LeggingsRecord>
    {
        public LeggingsRecordProcessorPoq(ItemRecordsControllerPoq controller) : base(controller) { }
    }
}