using pl.breams.SimpleDOTSUndo.Components;
using pl.breams.SimpleDOTSUndo.Sample.Components;
using pl.breams.SimpleDOTSUndo.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace pl.breams.SimpleDOTSUndo.Sample.Systems
{
    [UpdateInGroup(typeof(UndoSystemGroup))]
    [UpdateBefore(typeof(UndoSystem))]
    public partial class InputSystem:SystemBase
    {
        private EntityArchetype _UndoArchetype;
        private EntityArchetype _RedoArchetype;
        private EntityArchetype _MoveEntityCommandArchetype;
        private EntityArchetype _CancelArchetype;
        private EntityArchetype _ConfirmArchetype;
        private bool _Canceled = false;
        protected override void OnCreate()
        {
            base.OnCreate();
            //var ug = World.GetOrCreateSystemManaged<MyFixedUpdateGroup>();
            //ug.Timestep = 1;
            _UndoArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<PerformUndo>());
            _RedoArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<PerformDo>());
            _MoveEntityCommandArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<MoveEntityCommand>(), ComponentType.ReadWrite<RollbackMoveEntityCommand>(), ComponentType.ReadWrite<Command>(), ComponentType.ReadWrite<TempCommand>());
            _CancelArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<CancelCommand>());
            _ConfirmArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<ConfirmCommand>());
        }

        protected override void OnUpdate()
        {
            //Debug.Log("M:"+SystemAPI.Time.ElapsedTime);
            var playerEntity = SystemAPI.GetSingletonEntity<Player>();
            var translation = SystemAPI.GetAspectRW<TransformAspect>(playerEntity);
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) &&
                !Input.GetKey(KeyCode.LeftShift) && Input.GetKeyUp(KeyCode.Z))
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
                Input.GetKeyUp(KeyCode.Z))
            {
                EntityManager.CreateEntity(_RedoArchetype);
                if(Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Alpha3))
                    EntityManager.CreateEntity(_RedoArchetype);
                if(Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Alpha3))
                    EntityManager.CreateEntity(_RedoArchetype);
                if(Input.GetKey(KeyCode.Alpha3))
                    EntityManager.CreateEntity(_RedoArchetype);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _Canceled = true;
                EntityManager.CreateEntity(_CancelArchetype);
            }

            if (GetMoveInput(out var moveDir))
            {
                var currentPosition = translation.LocalPosition;
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
                EntityManager.CreateEntity(_RedoArchetype);

            }

            if (GetMoveInputConfirm())
            {
                if (_Canceled)
                    _Canceled = false;
                else if (SystemAPI.HasSingleton<TempCommand>())
                {
                    EntityManager.CreateEntity(_ConfirmArchetype);
                }
            }

        }

        private bool GetMoveInput(out float3 moveDir)
        {
            moveDir = float3.zero;

            if (Input.GetKeyDown(KeyCode.W))
            {
                UnityEngine.Debug.Log("W");
                moveDir = new float3(0, 0, 1);
                return true;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                UnityEngine.Debug.Log("S");
                moveDir = new float3(0, 0, -1);
                return true;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                UnityEngine.Debug.Log("A");
                moveDir = new float3(-1, 0, 0);
                return true;
                
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                UnityEngine.Debug.Log("D");
                moveDir = new float3(1, 0, 0);
                return true;
            }
            return false;
        }

        private bool GetMoveInputConfirm()
        {

            if (Input.GetKeyUp(KeyCode.W))
                return true;
            if (Input.GetKeyUp(KeyCode.S))
                return true;
            if (Input.GetKeyUp(KeyCode.A))
                return true;
            if (Input.GetKeyUp(KeyCode.D))
                return true;

            return false;
        }
    }

}