using Unity.Entities;

namespace zc.game.unit
{
	public class PlayerUnitAuthoring : BaseUnitAuthoring
	{
		public bool isPlayer;
		
		public class PlayerUnitBaker : Baker<PlayerUnitAuthoring>
		{
			public override void Bake(PlayerUnitAuthoring inAuthoring)
			{
				var entity_ = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent(entity_, new PlayerUnitMoveComponent()
				{
					speed = inAuthoring.speed
				});

				if (inAuthoring.isPlayer)
				{
					AddComponent(entity_, new Player());
				}
				
			}
		}
		
	}
}
