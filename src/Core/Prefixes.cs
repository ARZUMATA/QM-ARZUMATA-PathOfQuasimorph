using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.Core
{
    internal class Prefix
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Prefix(string type, string prefix, string description)
        {
            Type = type;
            Name = prefix;
            Description = description;
        }

        public static class AffixDatabase
        {
            public static Dictionary<string, Prefix> Prefixes { get; } = new Dictionary<string, Prefix>
    {
            // Ranged Weapon Prefixes
            {"RangedWeaponDamage1", new Prefix("Prefix", "Deadly", "Adds 1 to 2 Physical Damage to Attacks")},

            };
            }
    }
}
