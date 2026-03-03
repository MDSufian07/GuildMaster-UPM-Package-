namespace GuildMaster.Systems.SaveManager
{

    public class SaveData
    {
        public int GuildCoins { get; set; }
        public int GuildReputation { get; set; }
        public int GuildLevel { get; set; }
        public List<AdventurerData> Adventurers { get; set; } = new();
    }

    public class AdventurerData
    {
        public string? Name { get; set; }
        public int Level { get; set; }
        public int XP { get; set; }
        public int CostPerMission { get; set; }
        public string? Specialty { get; set; }
        public bool IsInjured { get; set; }
        public int RecoveryTime { get; set; }
        public string? Status { get; set; }

        // Stats
        public int Combat { get; set; }
        public int Defense { get; set; }
        public int Intelligence { get; set; }
        public int Agility { get; set; }
        public int Charisma { get; set; }
    }
}