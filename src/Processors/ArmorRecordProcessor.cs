using MGSC;
using QM_PathOfQuasimorph.Controllers;

namespace QM_PathOfQuasimorph.Processors
{
    internal class ArmorRecordProcessor<T> : ResistItemProcessor<T> where T : ArmorRecord
    {
        public ArmorRecordProcessor(ItemRecordsControllerPoq controller) : base(controller) { }
    }
}