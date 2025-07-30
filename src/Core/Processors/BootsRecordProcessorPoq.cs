using MGSC;

namespace QM_PathOfQuasimorph.Core.Processors
{
    internal class BootsRecordProcessorPoq : ResistItemProcessor<BootsRecord>
    {
        public BootsRecordProcessorPoq(ItemRecordsControllerPoq controller) : base(controller) { }
    }
}