using System;
using Random = System.Random;

public class BlackJackRoller
{
    private readonly Random _random = Helpers._random;
    private int _currentScore = 0;
    private readonly int _maxLevel;
    private readonly int _bustThreshold;

    // Dealer Mood System
    private float _moodValue = 50f; // 0 = furious (harsh), 100 = delighted (generous)
    private const float MoodDriftRate = 0.5f; // Slowly changes over time
    private const float CheatingThreshold = 35f; // Below this, dealer starts cheating for low rolls
    private readonly bool _enableCheating;
    
    public BlackJackRoller(int maxLevel, bool enableCheating = false)
    {
        _maxLevel = maxLevel;
        _bustThreshold = 20;
        _enableCheating = enableCheating;
    }

    public (int newLevel, bool wasCritical, bool didBust) Draw(bool forceCritical = false)
    {
        // Mood slowly drifts toward neutral
        if (_moodValue > 50)
        {
            _moodValue -= MoodDriftRate;
        }
        else if (_moodValue < 50)
        {
            _moodValue += MoodDriftRate;
        }

        // Auto-force crit if near bust
        if (!forceCritical && _currentScore > _bustThreshold * 0.8)
        {
            forceCritical = true;
        }

        // Simulate card draw: 1-10 (A, 2-10), J/Q/K = 10, A = 1 or 11
        int cardValue = DrawCard();

        // Subtle cheating: if mood is low, tweak the card
        if (_enableCheating && _moodValue < CheatingThreshold)
        {
            float cheatRoll = _random.Next(0, 100);
            
            // 30% chance to sabotage card when moody
            if (cheatRoll < 30)
            {
                if (cardValue > 8)
                {
                    // Downgrade high cards (9,10,11) to 2-5
                    cardValue = _random.Next(2, 6);
                }
                else if (_currentScore + cardValue >= 18 && _currentScore + cardValue <= 20)
                {
                    // If about to hit high tier, push over (bust) or under
                    if (_random.Next(0, 10) < 5)
                    {
                        cardValue = _random.Next(1, 3); // Nerf
                    }
                    else
                    {
                        cardValue += 5; // Push into bust
                    }
                }
            }
        }

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
            else if (riskRoll < 70) // 30% chance to downgrade
            {
                level = Math.Max(0, level - _random.Next(1, 3));
            }
            // else 30% chance: keep level but reset score
            _currentScore = 0;
            didBust = true;

            // Dealer gets more irritated after a player success before bust
            if (isCritical || cardValue > 10)
            {
                _moodValue = Math.Max(0, _moodValue - 10); // Annoyed by good luck
            }
        }
        else
        {
            // If player got a good safe roll, dealer gets slightly annoyed
            if (_currentScore >= 15 && _currentScore <= 17)
            {
                _moodValue = Math.Max(0, _moodValue - 5);
            }
        }

        // After critical, dealer might be amused
        if (isCritical)
        {
            _moodValue = Math.Min(100, _moodValue + 3);
        }

        return (level, isCritical, didBust);
    }

    private int DrawCard()
    {
        int roll = _random.Next(1, 14); // 1 (A) to 13 (K)

        if (roll == 1) // Ace: 1 or 11
        {
            // Bias Ace toward 1 if mood is bad
            if (_enableCheating && _moodValue < CheatingThreshold && _random.Next(0, 10) < 6)
            {
                return 1;
            }
            else
            {
                return _random.Next(0, 2) == 0 ? 1 : 11;
            }
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