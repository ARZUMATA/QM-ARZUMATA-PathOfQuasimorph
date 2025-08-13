using System;
using static QM_PathOfQuasimorph.Controllers.CreaturesControllerPoq;

namespace QM_PathOfQuasimorph.Contexts
{
    internal partial class PathOfQuasimorph
    {
        public static class MobContext
        {
            [ThreadStatic]
            internal static int CurrentMobId = -1;
            internal static MonsterMasteryTier Rarity = MonsterMasteryTier.None;
            internal static bool ProcesingMobRarity = false;
        }
    }
}
