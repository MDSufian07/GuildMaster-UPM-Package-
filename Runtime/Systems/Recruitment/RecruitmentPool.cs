using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Core.Entities;
using GuildMaster.Core.Enums;
using GuildMaster.Core.ValueObjects;

namespace GuildMaster.Systems.Recruitment
{

    public class RecruitmentPool
    {
        private readonly Random _random = new Random();

        private readonly string[] _namePool =
        {
            "Thorin", "Lyra", "Gareth", "Freya", "Baldric", "Isolde", "Cedric", "Mira",
            "Roland", "Seraphina", "Viktor", "Astrid", "Magnus", "Elise", "Draven", "Cara"
        };

        public List<RecruitmentCandidate> GenerateCandidates(int guildLevel, int count = 3)
        {
            var candidates = new List<RecruitmentCandidate>();
            var usedNames = new HashSet<string>();

            for (int i = 0; i < count; i++)
            {
                string name = GetUniqueName(usedNames);
                SkillLevel skillLevel = GenerateSkillLevel(guildLevel);
                SpecialtyType specialty = GetRandomSpecialty();

                var stats = GenerateStats(skillLevel);
                int recruitmentCost = CalculateRecruitmentCost(skillLevel, stats);

                var adventurer = new Adventurer(name, stats, specialty);
                candidates.Add(new RecruitmentCandidate(adventurer, skillLevel, recruitmentCost));
            }

            return candidates;
        }

        private string GetUniqueName(HashSet<string> usedNames)
        {
            string name;
            do
            {
                name = _namePool[_random.Next(_namePool.Length)];
            } while (usedNames.Contains(name));

            usedNames.Add(name);
            return name;
        }

        private SkillLevel GenerateSkillLevel(int guildLevel)
        {
            // Higher guild level = chance for better recruits
            int roll = _random.Next(1, 101);

            if (guildLevel >= 5 && roll > 80) return SkillLevel.Veteran;
            if (guildLevel >= 4 && roll > 60) return SkillLevel.Skilled;
            if (guildLevel >= 3 && roll > 40) return SkillLevel.Competent;
            if (guildLevel >= 2 && roll > 50) return SkillLevel.Novice;

            return SkillLevel.Trainee;
        }

        private SpecialtyType GetRandomSpecialty()
        {
            var specialties = (SpecialtyType[])Enum.GetValues(typeof(SpecialtyType));
            return specialties[_random.Next(specialties.Length)];
        }

        private Dictionary<StatType, int> GenerateStats(SkillLevel skillLevel)
        {
            int min, max;

            switch (skillLevel)
            {
                case SkillLevel.Trainee:
                    min = 1;
                    max = 3;
                    break;
                case SkillLevel.Novice:
                    min = 2;
                    max = 4;
                    break;
                case SkillLevel.Competent:
                    min = 3;
                    max = 6;
                    break;
                case SkillLevel.Skilled:
                    min = 5;
                    max = 8;
                    break;
                case SkillLevel.Veteran:
                    min = 7;
                    max = 10;
                    break;
                default:
                    min = 1;
                    max = 3;
                    break;
            }

            return new Dictionary<StatType, int>
            {
                { StatType.Combat, _random.Next(min, max + 1) },
                { StatType.Defense, _random.Next(min, max + 1) },
                { StatType.Intelligence, _random.Next(min, max + 1) },
                { StatType.Agility, _random.Next(min, max + 1) },
                { StatType.Charisma, _random.Next(min, max + 1) }
            };
        }

        private int CalculateRecruitmentCost(SkillLevel skillLevel, Dictionary<StatType, int> stats)
        {
            int baseCost = skillLevel switch
            {
                SkillLevel.Trainee => 50,
                SkillLevel.Novice => 100,
                SkillLevel.Competent => 200,
                SkillLevel.Skilled => 350,
                SkillLevel.Veteran => 500,
                _ => 50
            };

            int totalStats = stats.Values.Sum();
            return baseCost + (totalStats * 5);
        }
    }
}