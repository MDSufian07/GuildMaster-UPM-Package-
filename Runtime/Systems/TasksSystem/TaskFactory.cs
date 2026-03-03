using System.Collections.Generic;
using GuildMaster.Core.Entities;
using GuildMaster.Core.ValueObjects;

namespace GuildMaster.Systems.TasksSystem
{

    public class TaskFactory : ITaskFactory
    {
        public Core.Entities.Task Create(string name,
            Dictionary<StatType, int> stats,
            int difficulty,
            int baseReward,
            int baseXp)
        {
            return new Core.Entities.Task(name, stats, difficulty, baseReward, baseXp);
        }
    }
}