using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.Core
{
    [ConsoleCommand(new string[] { "poqcleanup" })]
    internal class ConsoleCommandPoqClean
    {
        public static string Help(string command, bool verbose)
        {
            return "Tries to clean all the POQ related data from the save before saving.";
        }

        public string Execute(string[] tokens)
        {
            CleanupSystem.SetCleanupMode(true);
            CleanupSystem.CleanObsoleteProjects(PathOfQuasimorph._context, true, true);
            CleanupSystem.SetCleanupMode(false);

            return "Done... Probably...";
        }

        public static List<string> FetchAutocompleteOptions(string command, string[] tokens)
        {
            return null;
        }

        public static bool IsAvailable()
        {
            return true;
        }

        public static bool ShowInHelpAndAutocomplete()
        {
            return true;
        }

        public ConsoleCommandPoqClean() { }
    }
}
