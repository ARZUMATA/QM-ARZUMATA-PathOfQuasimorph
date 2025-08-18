using System;
using Random = System.Random;

public class BlackJackRoller
{
    private readonly Random _random = Helpers._random;
    private int _currentScore = 0;
    private readonly int _maxLevel;
    private readonly int _bustThreshold;

    public BlackJackRoller(int maxLevel)
    {
        _maxLevel = maxLevel;
        _bustThreshold = 20;
    }

    public (int newLevel, bool wasCritical, bool didBust) Draw(bool forceCritical = false)
    {
        // Auto-force crit if near bust
        if (!forceCritical && _currentScore > _bustThreshold * 0.8)
        {
            forceCritical = true;
        }

        // Simulate card draw: 1-10 (A, 2-10), J/Q/K = 10, A = 1 or 11
        int cardValue = DrawCard();

        bool isCritical = forceCritical || IsCriticalCard(cardValue);
        bool didBust = false;

        // Apply card value
        _currentScore += cardValue;

        int level = ScoreToLevel(_currentScore);

        // Handle critical effects (chaotic jumps)
        if (isCritical)
        {
            int jump = _random.Next(1, 3); // +1 or +2 only
            level = WrapLevel(level + jump);
        }

        // Bust check: over threshold > reset or downgrade
        if (_currentScore > _bustThreshold)
        {
            int riskRoll = _random.Next(0, 100);
            if (riskRoll < 30) // 30% chance to reset
            {
                level = 0;
                _currentScore = 0;
            }
            else if (riskRoll < 70) // 40% chance to downgrade
            {
                level = Math.Max(0, level - _random.Next(1, 3));
            }
            // else 30% chance: keep level but reset score
            _currentScore = 0;
            didBust = true;
        }

        return (level, isCritical, didBust);
    }

    private int DrawCard()
    {
        // Simulate 52-card deck (we don't track suits, just values)
        int roll = _random.Next(1, 14); // 1 (A) to 13 (K)

        if (roll == 1) // Ace: 1 or 11
        {
            return _random.Next(0, 2) == 0 ? 1 : 11;

        }
        else if (roll > 10) // J, Q, K = 10
        {
            return 10;

        }
        else
        {
            return roll;
        }
    }

    private bool IsCriticalCard(int value) => value == 11 || (value == 10 && _random.Next(0, 10) < 3); // Ace(11) + 40% of 10s

    private int ScoreToLevel(int score)
    {
        if (score < 10) return 0;  // Standard
        if (score < 15) return 1;  // Enhanced
        if (score < 18) return 2;  // Advanced
        if (score < 22) return 3;  // Premium
        if (score < 28) return 4;  // Prototype
        return 5;                  // Quantum (28+)
    }

    private int WrapLevel(int level) => (level % (_maxLevel + 1) + (_maxLevel + 1)) % (_maxLevel + 1);

}