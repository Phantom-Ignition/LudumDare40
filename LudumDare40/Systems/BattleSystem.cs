using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LudumDare40.Components.Battle;
using LudumDare40.Components.Sprites;
using Nez;

namespace LudumDare40.Systems
{
    class BattleSystem : EntityProcessingSystem
    {
        public BattleSystem() : base(new Matcher().all(typeof(BattleComponent), typeof(AnimatedSprite), typeof(BoxCollider))) { }

        public override void process(Entity entity)
        {
            var sprite = entity.getComponent<AnimatedSprite>();
            if (sprite.getCurrentAnimation().FramesToAttack.Contains(sprite.CurrentFrame))
            {
                foreach (var otherEntity in _entities)
                {
                    if (otherEntity == entity) continue;
                    var otherBattler = otherEntity.getComponent<BattleComponent>();
                    if (otherBattler.ImmunityTime > 0.0f) continue;
                    var collider = otherEntity.getComponent<BoxCollider>();
                    foreach (var attackCollider in sprite.getCurrentFrame().AttackColliders)
                    {
                        CollisionResult collisionResult;
                        if (attackCollider.collidesWith(collider, out collisionResult))
                        {
                            otherBattler.onHit(collisionResult);
                        }
                    }
                }
            }
        }
    }
}
