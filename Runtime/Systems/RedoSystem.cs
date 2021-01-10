using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AddCommandSystem))]
    public class RedoSystem:SystemBase
    {
        protected override void OnUpdate()
        {

        }
    }
}