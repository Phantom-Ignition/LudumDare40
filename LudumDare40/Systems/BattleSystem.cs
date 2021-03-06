﻿using LudumDare40.Components.Battle;
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
                    if (otherBattler.isOnImmunity() || otherBattler.Dying || !otherBattler.battleEntity.canTakeDamage) continue;
                    var collider = getBattleCollider(otherEntity);
                    foreach (var attackCollider in sprite.getCurrentFrame().AttackColliders)
                    {
                        CollisionResult collisionResult;
                        if (attackCollider.collidesWith(collider, out collisionResult))
                        {
                            // Freeze time
                            Time.timeScale = 0.2f;
                            Core.schedule(0.01f, t =>
                            {
                                Time.timeScale = 1;
                            });
                            otherBattler.onHit(collisionResult);
                        }
                    }
                }
            }
        }

        private BoxCollider getBattleCollider(Entity entity)
        {
            var hurtCollider = entity.getComponent<HurtCollider>();
            return hurtCollider ?? entity.getComponent<BoxCollider>();
        }
    }
}
