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
    internal class BackpackRecordProcessor<T> : BreakableItemProcessor<T> where T : BackpackRecord
    {
        //private new Logger _logger = new Logger(null, typeof(BackpackRecordProcessor<T>));

        public override Dictionary<string, bool> parameters => _parameters;

        public BackpackRecordProcessor(ItemRecordsControllerPoq itemRecordsControllerPoq) : base(itemRecordsControllerPoq)
        {
            // Extend the base parameters
            _parameters["ReloadTurnMod"] = false;
            _parameters["Height"] = true;
            _parameters["AddServoArm"] = true;
            _parameters["BackpackWeightMult"] = false;
        }

        protected override void ApplyStat(float finalModifier, bool increase, KeyValuePair<string, bool> stat, T genericRecord = null)
        {
            // Simply for logging
            float outOldValue = -1;
            float outNewValue = -1;

            // If we got declared generic we take their values for reroll, and if not, use it as actual item record.
            if (genericRecord == null)
            {
                genericRecord = itemRecord;
            }

            switch (stat.Key)
            {
                case "ReloadTurnMod":
                    PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.ReloadTurnMod = v, () => genericRecord.ReloadTurnMod, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "Height":
                    PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.Height = v, () => genericRecord.Height, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "AddServoArm":
                    //PathOfQuasimorph.raritySystem.Apply<bool>(v => itemRecord.AddServoArm = v, () => genericRecord.AddServoArm, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "BackpackWeightMult":
                    PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.BackpackWeightMult = v, () => genericRecord.BackpackWeightMult, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                default:
                    // For all other stats (resists, weight, durability), use base logic
                    base.ApplyStat(finalModifier, increase, stat, genericRecord);
                    return;
            }

            Plugin.Logger.Log($"\t\t old value {outOldValue}");
            Plugin.Logger.Log($"\t\t new value {outNewValue}");
        }
    }
}
