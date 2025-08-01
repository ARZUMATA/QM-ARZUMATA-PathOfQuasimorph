using MGSC;
using System;
using static QM_PathOfQuasimorph.Core.CreaturesControllerPoq;
using static QM_PathOfQuasimorph.Core.RecombinatorController;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        public static class RecombinatorContext
        {
            [ThreadStatic]
            public static BasePickupItem Item;
            public static bool Process = false;
            public static RecombinatorType RecombinatorType;
            public static GameLoopGroup GameLoopGroup;
        }
    }
}
