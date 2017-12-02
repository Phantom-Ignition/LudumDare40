using System.Collections.Generic;
using LudumDare40.Components.Map;
using LudumDare40.Scenes;
using Nez;

namespace LudumDare40.Systems
{
    class TransferSystem : EntityProcessingSystem
    {
        private Entity _player;
        private bool _enabled;

        public TransferSystem(Matcher matcher, Entity player) : base(matcher)
        {
            _player = player;
            _enabled = true;
        }

        protected override void process(List<Entity> entities)
        {
            if (!_enabled) return;
            base.process(entities);
        }

        public override void process(Entity entity)
        {
            CollisionResult collisionResult;
            if (entity.getComponent<Collider>().collidesWith(_player.getComponent<Collider>(), out collisionResult))
            {
                var sceneMap = (SceneMap)_scene;
                sceneMap.reserveTransfer(entity.getComponent<TransferComponent>());
                _enabled = false;
                return;
            }
        }
    }
}
