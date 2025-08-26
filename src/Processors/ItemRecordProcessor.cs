using MGSC;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Core;
using QM_PathOfQuasimorph.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MGSC.TurnDebugLogger;

namespace QM_PathOfQuasimorph.Processors
{
    internal abstract class ItemRecordProcessor<T> : BasePickupItemRecordProcessor<T> where T : ItemRecord
    {
        //private new Logger _logger = new Logger(null, typeof(ItemRecordProcessor<T>));
        public override Dictionary<string, bool> parameters => _parameters;

        protected ItemRecordProcessor(ItemRecordsControllerPoq itemRecordsControllerPoq) : base(itemRecordsControllerPoq)
        {
        }
    }
}