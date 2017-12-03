using LudumDare40.Components.Battle;
using Nez;
using Nez.ECS.Components.Physics.Colliders;

namespace LudumDare40.Systems
{
    class ShotsBattleSystem : EntityProcessingSystem
    {
        private readonly BattleComponent _playerBattler;
        private readonly BoxCollider _playerCollider;

        public ShotsBattleSystem(Entity player) : base(new Matcher().all(typeof(ShotComponent), typeof(BoxCollider)))
        {
            _playerBattler = player.getComponent<BattleComponent>();
            _playerCollider = player.getComponent<BoxCollider>();
        }

        public override void process(Entity entity)
        {
            CollisionResult collisionResult;
            var collider = entity.getComponent<Collider>();
            if (collider.collidesWithAnyOfType<MapBoxCollider>(out collisionResult))
            {
                entity.destroy();
            }

            if (_playerBattler.isOnImmunity()) return;
            if (collider.collidesWith(_playerCollider, out collisionResult))
            {
                _playerBattler.onHit(collisionResult);
            }
        }
    }
}
