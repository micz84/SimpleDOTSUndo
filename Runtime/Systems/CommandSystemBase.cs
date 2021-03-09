using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Systems
{
    [DisableAutoCreation]
    public class CommandSystemBase:SystemBase
    {
        protected override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }
    }
}