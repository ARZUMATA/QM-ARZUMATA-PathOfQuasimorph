using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.Core
{
    internal class Affixes
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Affixes(string type, string affix, string description)
        {
            Type = type;
            Name = affix;
            Description = description;
        }
    }
}
