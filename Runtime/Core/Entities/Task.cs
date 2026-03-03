using System.Collections.Generic;
using System.Linq;
using GuildMaster.Core.Enums;
using GuildMaster.Core.ValueObjects;

namespace GuildMaster.Core.Entities
{
    public class Task
    {
        public Dictionary<StatType, int> RequiredStats { get; }
        public int Difficulty { get; }
        public int BaseReward { get; }
        public int BaseXP { get; }
        public string Name { get; }

        public Task(string name,
            Dictionary<StatType, int> requiredStats,
            int difficulty,
            int baseReward,
            int baseXp)
        {
            Name = name;
            RequiredStats = requiredStats;
            Difficulty = difficulty;
            BaseReward = baseReward;
            BaseXP = baseXp;
        }

        public float GetRequiredAverage()
        {
            return (float)RequiredStats.Values.Average();
        }
    }
}