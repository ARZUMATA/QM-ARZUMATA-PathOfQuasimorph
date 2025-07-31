using MGSC;
using System;
using static QM_PathOfQuasimorph.Core.CreaturesControllerPoq;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        public static class RecombinatorContext
        {
            [ThreadStatic]
            public static BasePickupItem Item;
            public static bool Process = false;
        }
    }
}
