using System.Linq;
using System.Reflection;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    public class CommandSystemsRunnerSystem:SystemBase
    {
        protected override void OnCreate()
        {
            var pType = typeof(CommandSystemBase);
            Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && t != pType
                                      && pType.IsAssignableFrom(t));

        }

        protected override void OnUpdate()
        {

        }
    }
}