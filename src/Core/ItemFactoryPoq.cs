using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.Core
{
    internal static class ItemFactoryPoq
    {
        private static Logger _logger = new Logger(null, typeof(ItemFactoryPoq));

        // Reused original game code here.
        public static BasePickupItem CreateNewItem(string itemId, bool randomizeConditionAndCapacity = false)
        {
            var itemFactory = ItemFactory.Instance;

            CompositeItemRecord compositeItemRecord = Data.Items.GetRecord(itemId, false) as CompositeItemRecord;
            if (compositeItemRecord != null)
            {
                int inventoryWidthSize = ((ItemRecord)compositeItemRecord.PrimaryRecord).InventoryWidthSize;
                float weight = ((ItemRecord)compositeItemRecord.PrimaryRecord).Weight;
                PickupItem pickupItem = new PickupItem(itemId, inventoryWidthSize, weight);
                foreach (BasePickupItemRecord basePickupItemRecord in compositeItemRecord.Records)
                {
                    itemFactory.CreateComponent(pickupItem, itemFactory._componentsCache, basePickupItemRecord, randomizeConditionAndCapacity, compositeItemRecord.PrimaryRecord == basePickupItemRecord);
                    pickupItem.Add(basePickupItemRecord, compositeItemRecord.PrimaryRecord == basePickupItemRecord);
                    foreach (PickupItemComponent pickupItemComponent in itemFactory._componentsCache)
                    {
                        pickupItem.Add(pickupItemComponent);
                    }
                }
                itemFactory._componentsCache.Clear();
                return pickupItem;
            }

            _logger.LogError("Failed create item by record: '" + itemId.ToLog() + "'.");
            return null;
        }

    }
}
