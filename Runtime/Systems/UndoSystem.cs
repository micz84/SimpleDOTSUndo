using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AddCommandSystem))]
    public class UndoSystem:SystemBase
    {
        protected override void OnUpdate()
        {

        }
    }
}