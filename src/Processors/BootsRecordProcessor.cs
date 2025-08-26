using MGSC;
using QM_PathOfQuasimorph.Controllers;

namespace QM_PathOfQuasimorph.Processors
{
    internal class BootsRecordProcessor<T> : ResistItemProcessor<T> where T : BootsRecord
    {
        public BootsRecordProcessor(ItemRecordsControllerPoq controller) : base(controller) { }
    }
}