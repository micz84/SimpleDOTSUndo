using System;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Components
{
    [Serializable]
    public struct PreviousCommand:IComponentData
    {
        public Entity Entity;
    }
}