using MGSC;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Core;
using QM_PathOfQuasimorph.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MGSC.SpawnSystem;
using static MGSC.TurnDebugLogger;
using static QM_PathOfQuasimorph.Contexts.PathOfQuasimorph;
using static QM_PathOfQuasimorph.Core.PathOfQuasimorph;
using Random = System.Random;

namespace QM_PathOfQuasimorph.Processors
{
    internal abstract class BasePickupItemRecordProcessor<T> : ConfigTableRecordProcessor<T> where T : BasePickupItemRecord
    {
        //protected Logger _logger = new Logger(null, typeof(BasePickupItemRecordProcessor<T>));

        internal Dictionary<string, bool> _parameters = new Dictionary<string, bool>()
        {
        };



        internal BasePickupItemRecordProcessor(ItemRecordsControllerPoq itemRecordsControllerPoq) : base(itemRecordsControllerPoq)
        {
        }











        

        internal List<string> SelectWeightedTraits(Dictionary<string, int> traitWeights, int count, List<string> itemTraitsExisting, List<HashSet<string>> exclusiveGroups = null)
        {
            var availableTraits = traitWeights
                .Where(t => t.Value > 0) // Skip traits with 0 or negative weight
                .ToDictionary(t => t.Key, t => t.Value);

            // Normalize exclusiveGroups to avoid null checks
            var groups = exclusiveGroups;

            if (groups == null)
            {
                groups = new List<HashSet<string>>();
            }

            var selected = new List<string>();

            // for (int i = 0; i < count && availableTraits.Count > 0; i++)
            // {
            //     string selectedTrait = PathOfQuasimorph.raritySystem.SelectRarityWeighted<string>(availableTraits);
            //     selected.Add(selectedTrait);

            //     // Remove already selected trait to prevent duplicates
            //     availableTraits = availableTraits
            //         .Where(t => t.Key != selectedTrait)
            //         .ToDictionary(t => t.Key, t => t.Value);
            // }


            while (selected.Count < count && availableTraits.Count > 0)
            {
                // Select one trait using weighted randomness
                string selectedTrait = PathOfQuasimorph.raritySystem.SelectRarityWeighted<string>(availableTraits);

                // Remove the selected trait from pool
                availableTraits.Remove(selectedTrait);

                // Remove all conflicting traits from the same exclusive group
                for (int i = 0; i < groups.Count; i++)
                {
                    var group = groups[i];
                    if (group.Contains(selectedTrait))
                    {
                        // Remove all members of this group from available traits
                        foreach (var conflict in group)
                        {
                            availableTraits.Remove(conflict);
                        }
                        break;
                    }
                }

                // Only add the trait if it's not already in itemTraitsExisting
                if (!itemTraitsExisting.Contains(selectedTrait))
                {
                    selected.Add(selectedTrait);
                }

                // Even if it exists, we still remove it and it's group members as we don't need them no more.

            }

            return selected;
        }



        
    }
}