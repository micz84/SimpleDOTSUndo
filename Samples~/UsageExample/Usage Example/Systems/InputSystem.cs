using pl.breams.SimpleDOTSUndo.Components;
using pl.breams.SimpleDOTSUndo.Sample.Components;
using pl.breams.SimpleDOTSUndo.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace pl.breams.SimpleDOTSUndo.Sample.Systems
{
    [UpdateBefore(typeof(UndoSystemGroup))]
    [AlwaysUpdateSystem]
    public class InputSystem:SystemBase
    {
        private EntityArchetype _UndoArchetype;
        private EntityArchetype _RedoArchetype;
        private EntityArchetype _MoveEntityCommandArchetype;

        protected override void OnCreate()
        {
            base.OnCreate();
            _UndoArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<PerformUndo>());
            _RedoArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<PerformDo>());
            _MoveEntityCommandArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<MoveEntityCommand>(), ComponentType.ReadWrite<RollbackMoveEntityCommand>(), ComponentType.ReadWrite<PerformDo>(), ComponentType.ReadWrite<Command>());
        }

        protected override void OnUpdate()
        {
            var playerEntity = GetSingletonEntity<Player>();
            var translation = GetComponent<Translation>(playerEntity);
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) &&
                !Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Z))
            {
                EntityManager.CreateEntity(_UndoArchetype);
                if(Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Alpha3))
                    EntityManager.CreateEntity(_UndoArchetype);
                if(Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Alpha3))
                    EntityManager.CreateEntity(_UndoArchetype);
                if(Input.GetKey(KeyCode.Alpha3))
                    EntityManager.CreateEntity(_UndoArchetype);
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftAlt) &&
                Input.GetKeyDown(KeyCode.Z))
            {
                EntityManager.CreateEntity(_RedoArchetype);
                if(Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Alpha3))
                    EntityManager.CreateEntity(_RedoArchetype);
                if(Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Alpha3))
                    EntityManager.CreateEntity(_RedoArchetype);
                if(Input.GetKey(KeyCode.Alpha3))
                    EntityManager.CreateEntity(_RedoArchetype);
            }

            if (GetMoveInput(out var moveDir))
            {
                var currentPosition = translation.Value;
                var command = EntityManager.CreateEntity(_MoveEntityCommandArchetype);
                EntityManager.SetComponentData(command,new MoveEntityCommand
                {
                    Target = playerEntity,
                    Position = currentPosition + moveDir
                });
                EntityManager.SetComponentData(command,new RollbackMoveEntityCommand
                {
                    Position = currentPosition
                });
            }

        }

        private bool GetMoveInput(out float3 moveDir)
        {
            moveDir = float3.zero;
            if (Input.GetKeyDown(KeyCode.W))
            {
                moveDir = new float3(0, 0, 1);
                return true;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                moveDir = new float3(0, 0, -1);
                return true;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                moveDir = new float3(-1, 0, 0);
                return true;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                moveDir = new float3(1, 0, 0);
                return true;
            }

            return false;
        }
    }

}