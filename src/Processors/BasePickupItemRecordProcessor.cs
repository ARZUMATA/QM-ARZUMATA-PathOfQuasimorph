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











        





        
    }
}