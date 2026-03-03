using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Core.Entities;
using GuildMaster.Core.Enums;
using GuildMaster.Systems.Insurance;
using GuildMaster.Systems.AdventurerService;

namespace GuildMaster.Systems.Resolution
{

    public class TaskResolver : ITaskResolver
    {
        private readonly Random _random = new();

        public TaskResult ResolveTask(Core.Entities.Task task, List<Adventurer> team)
        {
            float successChance = CalculateTeamSuccessChance(task, team);

            int roll = _random.Next(0, 101);
            bool success = roll <= successChance;

            var xpGains = new Dictionary<Adventurer, int>();
            var dead = new List<Adventurer>();

            int coins;
            int reputation;

            if (success)
            {
                coins = (int)(task.BaseReward * (1.0f + successChance / 80f));
                // Reputation based on difficulty level and success chance
                reputation = task.Difficulty + (int)Math.Ceiling((successChance / 100f) * 2f);

                foreach (var adv in team)
                {
                    int xp = (int)(task.BaseXP * (1.0f + task.Difficulty * 0.2f));
                    xpGains.Add(adv, xp);

                    // Small chance of minor injury even on success (10% + 2% per difficulty)
                    if (CheckMinorInjury(task, adv))
                    {
                        int recoveryDays = _random.Next(1, 3); // 1-2 days recovery (minor)
                        InjuryService.ApplyInjury(adv, recoveryDays);
                    }
                }
            }
            else
            {
                coins = 0;
                reputation = -(task.Difficulty / 2);

                foreach (var adv in team)
                {
                    int xp = (int)(task.BaseXP * 0.2f);
                    xpGains.Add(adv, xp);

                    // Check for death (insurance can prevent this)
                    if (CheckDeath(task, adv))
                    {
                        // Premium insurance prevents death
                        if (adv.InsuranceType == InsuranceType.Premium)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"🛡️ {adv.Name}'s Premium Insurance prevented death!");
                            Console.ResetColor();
                        }
                        else
                        {
                            dead.Add(adv);
                        }
                    }
                    // Check for injury if not dead
                    else if (CheckSevereInjury(task, adv))
                    {
                        int recoveryDays = _random.Next(5, 10); // 5-9 days recovery (severe)
                        InjuryService.ApplyInjury(adv, recoveryDays);
                    }
                }
            }

            return new TaskResult(success, successChance, coins, reputation, xpGains, dead);
        }

        private float GetRequiredAverage(Core.Entities.Task task)
        {
            return (float)task.RequiredStats.Values.Average();
        }

        private float CalculateAdventurerSkill(Core.Entities.Task task, Adventurer adv)
        {
            float total = 0f;

            foreach (var stat in task.RequiredStats.Keys)
            {
                total += adv.GetStat(stat);
            }

            float avg = total / task.RequiredStats.Count;

            if (adv.Specialty.ToString() == task.Name)
                avg += 1f;

            if (adv.IsInjured)
                avg -= 2f;

            return avg;
        }

        private float CalculateTeamSkill(Core.Entities.Task task, List<Adventurer> team)
        {
            float total = 0f;

            foreach (var adv in team)
            {
                total += CalculateAdventurerSkill(task, adv);
            }

            return total;
        }

        private float CalculateTeamSuccessChance(Core.Entities.Task task, List<Adventurer> team)
        {
            float requiredAvg = GetRequiredAverage(task);
            float requiredTotal = requiredAvg * team.Count;

            float teamSkill = CalculateTeamSkill(task, team);

            if (requiredTotal == 0)
                return 0f;

            return (teamSkill / requiredTotal) * 100f;
        }

        private bool CheckDeath(Core.Entities.Task task, Adventurer adv)
        {
            float deathChance = task.Difficulty * 0.05f;

            if (adv.IsInjured)
                deathChance += 0.10f;

            return _random.NextDouble() < deathChance;
        }

        private bool CheckMinorInjury(Core.Entities.Task task, Adventurer adv)
        {
            // Low chance of minor injury on success (10% base + 2% per difficulty)
            float injuryChance = 0.10f + (task.Difficulty * 0.02f);

            if (adv.IsInjured)
                injuryChance += 0.05f; // Already injured, slightly higher chance

            return _random.NextDouble() < injuryChance;
        }

        private bool CheckSevereInjury(Core.Entities.Task task, Adventurer adv)
        {
            // High chance of severe injury on failure (35% base + 8% per difficulty)
            float injuryChance = 0.35f + (task.Difficulty * 0.08f);

            if (adv.IsInjured)
                injuryChance += 0.20f; // Already injured, much higher chance of reinjury

            return _random.NextDouble() < injuryChance;
        }
    }
}