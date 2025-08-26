namespace QM_PathOfQuasimorph.Processors
{
    internal abstract partial class ConfigTableRecordProcessor<T>
    {
        private interface IHasSlotType
        {
            string SlotType { get; }
        }
    }
}