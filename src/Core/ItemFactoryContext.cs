using System;
using static QM_PathOfQuasimorph.Core.CreaturesControllerPoq;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        public static class ItemFactoryContext
        {
            [ThreadStatic]
            internal static bool CanDo = true;
            internal static string Context = "None";
        }
    }
}
