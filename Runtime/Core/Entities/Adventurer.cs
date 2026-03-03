using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Core.Enums;
using GuildMaster.Core.ValueObjects;

namespace GuildMaster_UPM_Package.Runtime.Core.Entities
{
    [Serializable]
    public class Adventurer
    {
        public string Name;

        // Core Stats (0–10)
        private Dictionary<StatType, int> _stats;

        public IReadOnlyDictionary<StatType, int> Stats => _stats;

        public int Level { get; private set; }
        public int XP { get; private set; }

        public SpecialtyType Specialty { get; private set; }
        public int CostPerMission { get; private set; }
        public int RecruitmentCost { get; set; }

        public bool IsInjured { get; private set; }
        public int RecoveryTime { get; private set; }

        public GuildMaster_UPM_Package.Runtime.Core.Enums.AdventurerStatus Status { get; private set; }
        
        public GuildMaster_UPM_Package.Runtime.Core.Enums.InsuranceType InsuranceType { get; private set; }

        public Adventurer(string name,
            Dictionary<StatType, int> stats,
            SpecialtyType specialty)
        {
            Name = name;
            _stats = stats;
            Specialty = specialty;

            Level = 1;
            XP = 0;
            Status = GuildMaster_UPM_Package.Runtime.Core.Enums.AdventurerStatus.Available;
            InsuranceType = GuildMaster_UPM_Package.Runtime.Core.Enums.InsuranceType.None;

            UpdateCost();
        }

        public int GetStat(StatType type)
        {
            return _stats.TryGetValue(type, out var value) ? value : 0;
        }

        private void UpdateCost()
        {
            // Cost based on average stats
            float averageStat = (float)_stats.Values.Average();
            CostPerMission = 20 + (int)(averageStat * 10);
        }
    }
}