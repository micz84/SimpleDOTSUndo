using Unity.Entities;
using Unity.Mathematics;

namespace pl.breams.SimpleDOTSUndo.Sample.Components
{
    public struct MoveEntityCommand:IComponentData
    {
        public Entity Target;
        public float3 Position;
    }
    public struct RollbackMoveEntityCommand:IComponentData
    {
        public float3 Position;
    }
}