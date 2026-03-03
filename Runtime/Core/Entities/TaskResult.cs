using System.Collections.Generic;

namespace GuildMaster.Core.Entities
{
    public class TaskResult
    {
        public bool Success { get; }
        public float SuccessChance { get; }
        public int CoinsEarned { get; }
        public int ReputationChange { get; }

        public Dictionary<Adventurer, int> XPGains { get; }
        public List<Adventurer> DeadAdventurers { get; }

        public TaskResult(
            bool success,
            float successChance,
            int coins,
            int reputation,
            Dictionary<Adventurer, int> xpGains,
            List<Adventurer> dead)
        {
            Success = success;
            SuccessChance = successChance;
            CoinsEarned = coins;
            ReputationChange = reputation;
            XPGains = xpGains;
            DeadAdventurers = dead;
        }
    }
}