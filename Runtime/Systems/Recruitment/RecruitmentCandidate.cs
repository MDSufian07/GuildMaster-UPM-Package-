using GuildMaster.Core.Entities;
using GuildMaster.Core.Enums;

namespace GuildMaster.Systems.Recruitment
{

    public class RecruitmentCandidate
    {
        public Adventurer Adventurer { get; }
        public SkillLevel SkillLevel { get; }
        public int RecruitmentCost { get; }

        public RecruitmentCandidate(Adventurer adventurer, SkillLevel skillLevel, int recruitmentCost)
        {
            Adventurer = adventurer;
            SkillLevel = skillLevel;
            RecruitmentCost = recruitmentCost;
        }
    }
}