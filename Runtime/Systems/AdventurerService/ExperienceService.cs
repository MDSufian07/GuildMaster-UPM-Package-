using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Core.Entities;
using GuildMaster.Core.ValueObjects;

namespace GuildMaster.Systems.AdventurerService
{
    public static class ExperienceService
    {
        private static readonly Random _random = new();

        public static void AddXP(Adventurer adv, int amount)
        {
            if (adv == null) return;

            adv.GetType().GetProperty("XP")
                ?.SetValue(adv, adv.XP + amount);

            // Keep leveling up while XP is sufficient
            while (adv.XP >= GetXpRequiredForNextLevel(adv.Level))
            {
                LevelUp(adv);
            }
        }

        // Public method to get XP required for next level (can be called from UI)
        public static int GetXpRequiredForNextLevel(int currentLevel)
        {
            // Level 1→2: 100 XP
            // Level 2→3: 200 XP
            // Level 3→4: 300 XP
            // Formula: 100 + (currentLevel * 100)
            return 100 + (currentLevel * 100);
        }

        private static void LevelUp(Adventurer adv)
        {
            int xpRequired = GetXpRequiredForNextLevel(adv.Level);

            adv.GetType().GetProperty("Level")
                ?.SetValue(adv, adv.Level + 1);

            adv.GetType().GetProperty("XP")
                ?.SetValue(adv, adv.XP - xpRequired);

            // Upgrade stats based on specialty
            UpgradeStats(adv);
        }

        // Get stat changes from leveling up (for display purposes)
        public static Dictionary<StatType, int> GetStatChanges(Dictionary<StatType, int> oldStats, Dictionary<StatType, int> newStats)
        {
            var changes = new Dictionary<StatType, int>();
            foreach (var stat in newStats)
            {
                int oldValue = oldStats.ContainsKey(stat.Key) ? oldStats[stat.Key] : 0;
                int change = stat.Value - oldValue;
                if (change != 0)
                {
                    changes[stat.Key] = change;
                }
            }
            return changes;
        }

        private static void UpgradeStats(Adventurer adv)
        {
            var stats = adv.Stats;
            
            // Get reference to the private _stats dictionary using reflection
            var statsField = adv.GetType().GetField("_stats", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (statsField == null) return;

            var statsDictionary = (Dictionary<StatType, int>?)statsField.GetValue(adv);
            if (statsDictionary == null) return;

            // Determine which stats get boosted based on specialty
            var boostedStats = GetBoostedStatsBySpecialty(adv.Specialty);
            
            // +1 to boosted stats, +0-1 to other stats
            foreach (var statType in statsDictionary.Keys.ToList())
            {
                if (boostedStats.Contains(statType))
                {
                    statsDictionary[statType] += 1;  // Guaranteed +1
                }
                else if (_random.Next(0, 100) < 30)  // 30% chance for other stats
                {
                    statsDictionary[statType] += 1;
                }

                // Cap stats at 10
                if (statsDictionary[statType] > 10)
                    statsDictionary[statType] = 10;
            }
        }

        private static List<StatType> GetBoostedStatsBySpecialty(SpecialtyType specialty)
        {
            return specialty.ToString() switch
            {
                "Hunt" => new List<StatType> { StatType.Agility, StatType.Combat },
                "Guard" => new List<StatType> { StatType.Defense, StatType.Combat },
                "Cook" => new List<StatType> { StatType.Intelligence },
                "Craft" => new List<StatType> { StatType.Intelligence, StatType.Agility },
                _ => new List<StatType> { StatType.Combat }
            };
        }
    }
}