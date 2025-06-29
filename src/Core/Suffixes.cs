using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.Core
{
    internal class Suffix
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Suffix(string type, string suffix, string description)
        {
            Type = type;
            Name = suffix;
            Description = description;
        }
    }

    public static class SuffixDatabase
    {
    }
}
