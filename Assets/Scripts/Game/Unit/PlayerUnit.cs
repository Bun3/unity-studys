using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using zc.util;

namespace zc.game.unit 
{
	public struct Player : ITagComponent
	{
	}
	
	public struct PlayerUnitMoveComponent : IComponentData
	{
		public float speed;
	}
	
	[BurstCompile]
	public readonly partial struct PlayerUnitMoveAspect : IAspect
	{
		private readonly RefRW<LocalTransform> localTransform;

		private readonly RefRO<PlayerUnitMoveComponent> moveComponent;

		private float moveSpeed => moveComponent.ValueRO.speed;

		[BurstCompile]
		public void Move(float2 inMoveInput)
		{
			localTransform.ValueRW.Position.xy += inMoveInput * moveSpeed;
		}

	}
	
	[BurstCompile]
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
	public partial struct PlayerUnitMoveSystem : ISystem
	{
		public void OnCreate(ref SystemState inState)
		{
			inState.RequireForUpdate<InputComponent>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState inState)
		{
			var deltaTime_ = SystemAPI.Time.fixedDeltaTime;

			var inputComponent_ = inState.EntityManager.GetComponentData<InputComponent>(SystemAPI.GetSingletonEntity<InputComponent>());
			
			foreach (var playerUnitMove_ in SystemAPI.Query<PlayerUnitMoveAspect>())
			{
				playerUnitMove_.Move(inputComponent_.moveValue * deltaTime_);
			}
		}
	}
	
}
