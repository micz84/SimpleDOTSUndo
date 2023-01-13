using pl.breams.SimpleDOTSUndo.Sample.Components;
using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    
}

public class PlayerBaker : Baker<PlayerAuthoring>
{

    public override void Bake(PlayerAuthoring authoring)
    {
        AddComponent(new Player());
    }
}
