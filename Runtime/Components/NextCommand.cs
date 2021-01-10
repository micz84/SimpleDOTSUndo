using System;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Components
{
    [Serializable]
    public struct NextCommand:IComponentData
    {
        public Entity Entity;
    }
}