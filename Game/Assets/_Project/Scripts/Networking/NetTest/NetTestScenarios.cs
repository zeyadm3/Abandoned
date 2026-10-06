using System;
using System.Collections.Generic;

namespace Abandoned.Networking
{
    /// <summary>
    /// Name -> scenario. Later tasks add their own (loot pickup/throw, shared carry, collapse sync,
    /// disconnect) with one line in <see cref="CreateRegistry"/>, then run them with
    /// <c>Tools/nettest.sh &lt;name&gt;</c>.
    /// </summary>
    public static class NetTestScenarios
    {
        private static readonly Dictionary<string, Func<INetTestScenario>> Registry = CreateRegistry();

        private static Dictionary<string, Func<INetTestScenario>> CreateRegistry() =>
            new(StringComparer.OrdinalIgnoreCase)
            {
                { BasicNetTestScenario.ScenarioName, () => new BasicNetTestScenario() },
                { LootNetTestScenario.ScenarioName, () => new LootNetTestScenario() },
                { SharedCarryNetTestScenario.ScenarioName, () => new SharedCarryNetTestScenario() },
                { CollapseNetTestScenario.ScenarioName, () => new CollapseNetTestScenario() },
                { RobustNetTestScenario.ScenarioName, () => new RobustNetTestScenario() },
            };

        public static IEnumerable<string> Names => Registry.Keys;

        public static bool TryCreate(string name, out INetTestScenario scenario)
        {
            scenario = null;
            if (string.IsNullOrEmpty(name) || !Registry.TryGetValue(name, out Func<INetTestScenario> create)) return false;
            scenario = create();
            return true;
        }
    }
}
