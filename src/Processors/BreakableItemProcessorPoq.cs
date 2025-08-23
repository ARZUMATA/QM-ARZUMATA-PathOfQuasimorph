using MGSC;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Core;
using QM_PathOfQuasimorph.PoQHelpers;
using QM_PathOfQuasimorph.Records;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static MGSC.SpawnSystem;
using Random = System.Random;

namespace QM_PathOfQuasimorph.Processors
{
    internal class BreakableItemProcessorPoq : ItemRecordProcessor<BreakableItemRecord>
    {
        private new Logger _logger = new Logger(null, typeof(BreakableItemProcessorPoq));

        public override Dictionary<string, bool> parameters => _parameters;

        internal Dictionary<string, bool> _parameters = new Dictionary<string, bool>()
        {
        };


        public BreakableItemProcessorPoq(ItemRecordsControllerPoq itemRecordsControllerPoq) : base(itemRecordsControllerPoq)
        {
        }

        internal override void ProcessRecord(ref string boostedParamString)
        {
            AddUnbreakableTrait();
        }
        private bool AddUnbreakableTrait(float chanceOverride = 0)
        {
            if (itemRecord.Unbreakable)
            {
                return false;
            }

            var canAddUnbreakableTrait = false;

            // Only 20% of all items are eligible for unbreakable trait
            if (Helpers._random.NextDouble() <= PathOfQuasimorph.raritySystem.UNBREAKABLE_ENTRY_CHANCE &&
                PathOfQuasimorph.raritySystem.unbreakableTraitPercent.TryGetValue(itemRarity, out float weight) &&
                weight > 0)
            {
                // Get the list of eligible rarities and their weights
                var eligibleRarities = PathOfQuasimorph.raritySystem.unbreakableTraitPercent
                    .Where(kv => kv.Value > 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                // Calculate total weight among eligible rarities
                float totalWeight = eligibleRarities.Values.Sum();

                // Check if this specific item wins based on its weight
                if (Helpers._random.NextDouble() * totalWeight <= weight)
                {
                    canAddUnbreakableTrait = true;
                }
            }

            _logger.Log($"\t\t  Unbreakable: {canAddUnbreakableTrait}");

            if (chanceOverride > 0)
            {
                canAddUnbreakableTrait = Helpers._random.NextDouble() < chanceOverride;
            }

            if (canAddUnbreakableTrait)
            {
                itemRecord.Unbreakable = true;
            }
            else
            {
                itemRecord.Unbreakable = false;
            }

            return true;
        }

        internal bool AddUnbreakableTrait(SynthraformerRecord record, MetadataWrapper metadata, float chance)
        {
            return AddUnbreakableTrait(chance);
        }
    }
}
