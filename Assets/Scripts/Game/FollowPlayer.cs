using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using zc.game.unit;

namespace zc.game
{
	public class FollowPlayer : MonoBehaviour
	{
		[SerializeField] private Vector3 offset;
		
		private Entity playerEntity;
		private EntityQuery playerQuery;
		
		private void Start()
		{
			var entityManager_ = World.DefaultGameObjectInjectionWorld.EntityManager;
			playerQuery = entityManager_.CreateEntityQuery(typeof(Player), typeof(LocalTransform));
		}

		private void LateUpdate()
		{
			var playerPosition_ = playerQuery.GetSingleton<LocalTransform>().Position;
			transform.position = offset + (Vector3)playerPosition_;
		}
	}
    
}
