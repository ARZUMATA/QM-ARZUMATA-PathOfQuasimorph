using System;
using System.Linq;
public class BlackJackRollerSimulator
{
    public class RollStats
    {
        public int TotalRolls;
        public int[] LevelDistribution;
        public int[] Upgrades;
        public int[] Downgrades;
        public int Resets;
        public int Criticals;
        public int Busts;
        public long TotalLevelJump;
        public double AverageLevelJump => TotalRolls > 0 ? TotalLevelJump / (double)TotalRolls : 0;
    }

    public static RollStats RunSimulation(int maxLevel = 5, int rollCount = 100000)
    {
        var roller = new BlackJackRoller(maxLevel: maxLevel);
        var stats = new RollStats
        {
            TotalRolls = rollCount,
            LevelDistribution = new int[maxLevel + 1],
            Upgrades = new int[maxLevel + 1],
            Downgrades = new int[maxLevel + 1],
            Resets = 0,
            Criticals = 0,
            Busts = 0,
            TotalLevelJump = 0
        };

        int previousLevel = 0;

        for (int i = 0; i < rollCount; i++)
        {
            var (level, wasCritical, didBust) = roller.Draw();

            stats.LevelDistribution[level]++;

            if (wasCritical)
            {
                stats.Criticals++;
            }
            if (didBust)
            {
                stats.Busts++;
            }

            // Track upgrade/downgrade
            if (level == 0 && previousLevel != 0)
            {
                stats.Resets++;
            }
            else if (level > previousLevel)
            {
                stats.Upgrades[Math.Min(previousLevel, stats.Upgrades.Length - 1)]++;
            }
            else if (level < previousLevel)
            {
                stats.Downgrades[Math.Min(previousLevel, stats.Downgrades.Length - 1)]++;
            }

            stats.TotalLevelJump += level;
            previousLevel = level;
        }

        return stats;
    }

    public static void PrintReport(RollStats stats)
    {
        Console.WriteLine("========== BLACKJACK ROLLER: 100,000 ROLL STATISTICAL REPORT ==========");
        Console.WriteLine($"Total Rolls: {stats.TotalRolls:N0}");
        Console.WriteLine($"Average Resulting Level: {stats.AverageLevelJump:F2}");
        Console.WriteLine($"Critical Rolls: {stats.Criticals:N0} ({100.0 * stats.Criticals / stats.TotalRolls:F2}%)");
        Console.WriteLine($"Bust Events: {stats.Busts:N0} ({100.0 * stats.Busts / stats.TotalRolls:F2}%)");
        Console.WriteLine($"Resets (to Standard): {stats.Resets:N0} ({100.0 * stats.Resets / stats.TotalRolls:F2}%)");
        Console.WriteLine();
        Console.WriteLine("Level Distribution:");

        string[] levelNames = { "Standard", "Enhanced", "Advanced", "Premium", "Prototype", "Quantum" };

        for (int i = 0; i <= Math.Min(5, levelNames.Length - 1); i++)
        {
            int count = i < stats.LevelDistribution.Length ? stats.LevelDistribution[i] : 0;
            string name = i < levelNames.Length ? levelNames[i] : $"Level {i}";
            Console.WriteLine($"  {name}: {count:N0} ({100.0 * count / stats.TotalRolls:F2}%)");
        }

        for (int i = 6; i < stats.LevelDistribution.Length; i++)
        {
            int count = stats.LevelDistribution[i];
            Console.WriteLine($"  Level {i} (Wrapped): {count:N0} ({100.0 * count / stats.TotalRolls:F2}%)");
        }

        Console.WriteLine();
        Console.WriteLine("Upgrades from Level:");

        for (int i = 0; i < stats.Upgrades.Length; i++)
        {
            string name = i < levelNames.Length ? levelNames[i] : $"Level {i}";
            Console.WriteLine($"  From {name}: {stats.Upgrades[i]:N0}");
        }

        Console.WriteLine();
        Console.WriteLine("Downgrades from Level:");

        for (int i = 0; i < stats.Downgrades.Length; i++)
        {
            string name = i < levelNames.Length ? levelNames[i] : $"Level {i}";
            Console.WriteLine($"  From {name}: {stats.Downgrades[i]:N0}");
        }

        Console.WriteLine("========================================================================");
    }
}