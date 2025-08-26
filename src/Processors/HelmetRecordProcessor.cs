using MGSC;
using QM_PathOfQuasimorph.Controllers;

namespace QM_PathOfQuasimorph.Processors
{
    internal class HelmetRecordProcessor<T> : ResistItemProcessor<T> where T : HelmetRecord
    {
        public HelmetRecordProcessor(ItemRecordsControllerPoq controller) : base(controller) { }
    }
}