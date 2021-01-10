using System;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Components
{
    [Serializable]
    public struct DoComponent:IComponentData
    {
        public ComponentType ComponentType;
    }
}