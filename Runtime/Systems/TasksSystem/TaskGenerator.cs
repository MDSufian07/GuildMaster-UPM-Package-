using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Core.ValueObjects;

namespace GuildMaster.Systems.TasksSystem
{

    public class TaskGenerator
    {
        private readonly Random _random = new();

        public Core.Entities.Task GenerateRandomTask()
        {
            var stats = new Dictionary<StatType, int>
            {
                { StatType.Combat, _random.Next(1, 11) },
                { StatType.Agility, _random.Next(1, 11) },
                { StatType.Intelligence, _random.Next(1, 11) },
                { StatType.Defense, _random.Next(1, 11) },
                { StatType.Charisma, _random.Next(1, 11) }
            };

            // Calculate difficulty based on average of required stats
            double averageStats = stats.Values.Average();
            int difficulty = (int)Math.Ceiling(averageStats);

            // Calculate reward based on difficulty and total stats
            int totalStats = stats.Values.Sum();
            int reward = difficulty * 50 + totalStats * 10;

            // Calculate XP based on difficulty
            int xp = difficulty * 15 + totalStats * 2;

            return new Core.Entities.Task("Random Mission", stats, difficulty, reward, xp);
        }
    }
}