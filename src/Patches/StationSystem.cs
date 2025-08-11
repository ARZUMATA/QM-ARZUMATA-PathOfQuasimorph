using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(StationSystem), 
            nameof(StationSystem.AddItemToStationStorage),
            new Type[]
            {
                typeof(Mercenary),
                typeof(string),
                typeof(ItemStorage),
                typeof(bool),
            }
        )]
        public static class StationSystem_AddItemToStationStorage_Patch
        {
            public static void Prefix(SpaceTime spaceTime, Factions factions, Station station, ref BasePickupItem item)
            {
                /*
                 * When item is produced at station it's rarity is applied but we may not need rarity on the items so we need to create generic item.
                 * There is a "[GameLoopUpdate(GameLoopGroup.Space, GameLoopExecuteOrder.After, "SpaceTimeSystem.Update")]"
                 * public static void Update
                 * items are created there and added to internal storage, so without IL'patch there is no way to determine where item is created at, magnum, station, dungeon etc.
                 * So we intercept StationSystem.AddItemToStationStorage instead and since it's just a BasePickupItem we create new generic and replace the variable.
                 */

                var metadata = MetadataWrapper.SplitItemUid(item.Id);

                if (metadata != null && metadata.PoqItem)
                {
                    var baseId = metadata.Id;
                    item = ItemProductionSystem.ProduceItem(baseId);
                }
            }
        }
    }
}
