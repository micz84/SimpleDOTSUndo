using System;
using Unity.Entities;

namespace pl.breams.SimpleDOTSUndo.Components
{
    [Serializable]
    public struct UndoComponent:IComponentData
    {
        public ComponentType ComponentType;
    }
}