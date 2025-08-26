using MGSC;
using QM_PathOfQuasimorph.Controllers;

namespace QM_PathOfQuasimorph.Processors
{
    internal class LeggingsRecordProcessor<T> : ResistItemProcessor<T> where T : LeggingsRecord
    {
        public LeggingsRecordProcessor(ItemRecordsControllerPoq controller) : base(controller) { }
    }
}