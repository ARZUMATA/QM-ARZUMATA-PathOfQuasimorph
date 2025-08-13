using MGSC;
using System;
using static QM_PathOfQuasimorph.Controllers.CreaturesControllerPoq;
using static QM_PathOfQuasimorph.Controllers.SynthraformerController;

namespace QM_PathOfQuasimorph.Contexts
{
    internal partial class PathOfQuasimorph
    {
        public static class SynthraformerContext
        {
            [ThreadStatic]
            public static BasePickupItem Item;
            public static bool Process = false;
            public static SynthraformerType RecombinatorType;
            public static GameLoopGroup GameLoopGroup;
        }
    }
}
