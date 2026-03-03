using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Core.Enums;

namespace GuildMaster_UPM_Package.Runtime.Core.Entities
{
    public class Guild
    {
        public int Coins { get; private set; }
        public int Reputation { get; private set; }
        public int GuildLevel { get; private set; }
        private int _initialMaxAdventurers;
        public int MaxAdventurers 
        { 
            get { return _initialMaxAdventurers + ((GuildLevel - 1) * 2); }
        }
        public List<Adventurer> Adventurers { get; private set; }

        public Guild(int initialCoins = 500, int maxAdventurers = 3)
        {
            Coins = initialCoins;
            Reputation = 0;
            GuildLevel = 1;
            _initialMaxAdventurers = maxAdventurers;
            Adventurers = new List<Adventurer>();
        }

        public bool CanRecruit()
        {
            return Adventurers.Count < MaxAdventurers;
        }

        public bool TryRecruitAdventurer(Adventurer adventurer, int recruitmentCost)
        {
            if (!CanRecruit())
            {
                return false;
            }

            if (Coins < recruitmentCost)
            {
                return false;
            }

            Coins -= recruitmentCost;
            Adventurers.Add(adventurer);
            return true;
        }

        public void AddCoins(int amount)
        {
            Coins += amount;
        }

        public void AddReputation(int amount)
        {
            Reputation += amount;
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            int requiredReputation = GuildLevel * 10;
            if (Reputation >= requiredReputation)
            {
                int newSlots = MaxAdventurers + 2;
                GuildLevel++;
                // Use Debug if running in Unity; keep Console for non-Unity builds
                try { UnityEngine.Debug.Log($"🎉 GUILD LEVEL UP! Now Level {GuildLevel}!"); } catch { Console.WriteLine($"Guild level up: {GuildLevel}"); }
            }
        }

        public int GetAvailableAdventurerCount()
        {
            return Adventurers.Count(a => a.Status == AdventurerStatus.Available);
        }

        public int GetInjuredAdventurerCount()
        {
            return Adventurers.Count(a => a.IsInjured);
        }
    }
}
