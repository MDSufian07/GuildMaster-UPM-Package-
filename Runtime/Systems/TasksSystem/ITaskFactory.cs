using System.Collections.Generic;
using GuildMaster.Core.ValueObjects;

namespace GuildMaster.Systems.TasksSystem
{

    public interface ITaskFactory
    {
        Core.Entities.Task Create(string name,
            Dictionary<StatType, int> stats,
            int difficulty,
            int baseReward,
            int baseXp);
    }
}