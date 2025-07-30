using MGSC;

namespace QM_PathOfQuasimorph.Core.Processors
{
    internal class ArmorRecordProcessorPoq : ResistItemProcessor<ArmorRecord>
    {
        public ArmorRecordProcessorPoq(ItemRecordsControllerPoq controller) : base(controller) { }
    }
}