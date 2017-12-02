using System.Collections.Generic;
using LudumDare40.Components.Map;
using LudumDare40.Components.Player;
using Nez;

namespace LudumDare40.Systems
{
    public class LadderSystem : EntityProcessingSystem
    {
        private PlayerComponent _player;
        private LadderComponent _ladder;

        public LadderSystem(Matcher matcher, PlayerComponent player) : base(matcher)
        {
            _player = player;
        }

        protected override void process(List<Entity> entities)
        {
            _ladder = null;
            base.process(entities);
            if (_player.platformerObject != null)
            {
                _player.platformerObject.ladderComponent = _ladder;
            }
        }

        public override void process(Entity entity)
        {
            CollisionResult collisionResult;
            if (entity.getComponent<Collider>().collidesWith(_player.getComponent<Collider>(), out collisionResult))
            {
                _ladder = entity.getComponent<LadderComponent>();
            }
        }
    }
}
