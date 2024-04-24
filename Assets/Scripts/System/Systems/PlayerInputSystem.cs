using System.Runtime.InteropServices;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

using zc.core;
using zc.input;

namespace zc.system
{
    public class PlayerInputSystem : SystemBase<PlayerInputSystem>
    {
        #region Update Definitions
        
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct InputUpdate
        {
            
        }
        
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct InputUpdateBefore
        {
            
        }
        
        #endregion
        
        [ReadOnly]
        public Vector2 joystickInput;
        [ReadOnly] 
        [ShowInInspector]
        public Vector2 movementInput { get; private set; }

        #region EditorDebugVariables
#if UNITY_EDITOR
        
        [ReadOnly] 
        [ShowInInspector] 
        private float movementInputMagnitude => movementInput.magnitude;
        
#endif
        #endregion
        
        private readonly PlayerInputAsset playerInputAsset = new();

        private EntityManager entityManager;
        private Entity playerEntity;
        private EntityQuery playerQuery;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Application.quitting += Release;

            instance.Initialize_Internal();
        }
		
        private static void Release()
        {
            instance.Release_Internal();
			
            Application.quitting -= Release;
        }
        
        protected override void Initialize_Internal()
        {
            PlayerLoopSystemHelper.InsertSystemAfter(typeof(InputUpdate), OnInputUpdate, typeof(UnityEngine.PlayerLoop.PreUpdate.NewInputUpdate));
            PlayerLoopSystemHelper.InsertSystemBefore(typeof(InputUpdateBefore), OnInputUpdateBefore, typeof(UnityEngine.PlayerLoop.PreUpdate.NewInputUpdate));
            
            playerInputAsset.Enable();
            
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            playerEntity = entityManager.CreateSingleton<InputComponent>();
        }

        protected override void Release_Internal()
        {
            playerInputAsset.Disable();
        }

        private void OnInputUpdate()
        {
            //Set movement input before executing any other behaviour logic
            movementInput += joystickInput;
            movementInput += playerInputAsset.Essential.Move.ReadValue<Vector2>();

            movementInput = Vector2.ClampMagnitude(movementInput, 1f);

            entityManager.SetComponentData(playerEntity, new InputComponent
            {
                moveValue = movementInput
            });                
        }

        private void OnInputUpdateBefore()
        {
            movementInput = Vector2.zero;
        }
    }
    
}
