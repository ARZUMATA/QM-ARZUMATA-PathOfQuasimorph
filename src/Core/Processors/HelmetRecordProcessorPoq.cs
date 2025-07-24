using MGSC;

namespace QM_PathOfQuasimorph.Core.Processors
{
    internal class HelmetRecordProcessorPoq : ResistItemProcessor<HelmetRecord>
    {
        public HelmetRecordProcessorPoq(ItemRecordsControllerPoq controller) : base(controller) { }
    }
}