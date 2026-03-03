using System.Collections.Generic;
using GuildMaster.Core.Entities;

namespace GuildMaster.Systems.Resolution
{

    public interface ITaskResolver
    {
        TaskResult ResolveTask(Core.Entities.Task task, List<Adventurer> team);
    }
}