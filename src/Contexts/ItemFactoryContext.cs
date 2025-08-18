using System;
using static QM_PathOfQuasimorph.Controllers.CreaturesControllerPoq;

namespace QM_PathOfQuasimorph.Contexts
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
