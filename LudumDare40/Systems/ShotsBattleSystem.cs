using LudumDare40.Components.Battle;
using LudumDare40.Components.Map;
using LudumDare40.Components.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.ECS.Components.Physics.Colliders;
using System;
using System.Collections.Generic;
using LudumDare40.Managers;
using LudumDare40.Extensions;

namespace LudumDare40.Systems
{
    class ShotsBattleSystem : EntityProcessingSystem
    {
        private readonly AnimatedSprite _playerSprite;
        private readonly BattleComponent _playerBattler;
        private readonly BoxCollider _playerCollider;

        private Texture2D _bullectEffectTexture;
        private Texture2D _explosion2EffectTexture;
        private int _effectsCount;

        public ShotsBattleSystem(Entity player) : base(new Matcher().one(typeof(ShotComponent), typeof(MissileComponent)))
        {
            _playerSprite = player.getComponent<AnimatedSprite>();
            _playerBattler = player.getComponent<BattleComponent>();
            _playerCollider = player.getComponent<BoxCollider>();

            _bullectEffectTexture = player.scene.content.Load<Texture2D>(Content.Effects.bulletEffect);
            _explosion2EffectTexture = player.scene.content.Load<Texture2D>(Content.Effects.explosionEffect2);
        }

        public override void process(Entity entity)
        {
            var missile = entity.getComponent<MissileComponent>();

            // shots vs map
            CollisionResult collisionResult;
            var collider = entity.getComponent<Collider>();
            if (collider.collidesWithAnyOfType<MapBoxCollider>(out collisionResult))
            {
                if (missile != null)
                {
                    createMissileExplosionEffect(missile);
                }
                else
                {
                    createBulletEffect(entity.getComponent<ShotComponent>(), collisionResult.normal);
                }
                entity.destroy();
            }

            // player vs shots
            if (missile != null)
            {
                if (_playerSprite.getCurrentAnimation().FramesToAttack.Contains(_playerSprite.CurrentFrame))
                {
                    foreach (var attackCollider in _playerSprite.getCurrentFrame().AttackColliders)
                    {
                        if (attackCollider.collidesWith(collider, out collisionResult))
                        {
                            createMissileExplosionEffect(missile);
                            entity.destroy();
                        }
                    }
                }
            }

            // shots vs player
            if (_playerBattler.isOnImmunity()) return;
            if (collider.collidesWith(_playerCollider, out collisionResult))
            {
                if (missile != null) createMissileExplosionEffect(missile);
                _playerBattler.onHit(collisionResult);
                entity.destroy();
            }
        }
        
        private void createBulletEffect(ShotComponent bullet, Vector2 normal)
        {
            if (bullet == null) return;
            var effect = bullet.entity.scene.createEntity($"effect:playerComponent({getEffectsCount()})");
            var sprite = effect.addComponent(new AnimatedSprite(_bullectEffectTexture, "default"));
            sprite.CreateAnimation("default", 0.08f, false);
            sprite.AddFrames("default", new List<Rectangle>()
            {
                new Rectangle(0, 0, 10, 17),
                new Rectangle(10, 0, 10, 17),
                new Rectangle(20, 0, 10, 17),
            });
            var bulletSprite = bullet.getComponent<AnimatedSprite>();
            sprite.spriteEffects = bulletSprite.spriteEffects;
            var collider = bullet.entity.getComponent<BoxCollider>();
            effect.position = new Vector2(collider.bounds.center.X, collider.bounds.bottom - sprite.height / 2);
            effect.addComponent<SpriteEffectComponent>();

            var rotation = (float)Math.Atan2(normal.Y, normal.X);
            effect.rotation = rotation;
            if (sprite.getDirection() > 0)
            {
                effect.rotation = -rotation;
            }
            if (normal.Y != 0.0f) effect.position += 3 * Vector2.UnitY;
            if (normal.X == -1.0f)
            {
                sprite.flipX = true;
                effect.position += new Vector2(3 * sprite.getDirection(), 2);
            }
        }

        private void createMissileExplosionEffect(MissileComponent missile)
        {
            if (missile == null) return;
            AudioManager.explosion.Play(0.9f);
            var effect = missile.entity.scene.createEntity($"effect:playerComponent({getEffectsCount()})");
            var sprite = effect.addComponent(new AnimatedSprite(_explosion2EffectTexture, "default"));
            sprite.CreateAnimation("default", 0.08f, false);
            sprite.AddFrames("default", new List<Rectangle>()
            {
                new Rectangle(0, 0, 32, 32),
                new Rectangle(32, 0, 32, 32),
                new Rectangle(64, 0, 32, 32),
                new Rectangle(96, 0, 32, 32),
                new Rectangle(128, 0, 32, 32),
            });
            var collider = missile.entity.getComponent<BoxCollider>();
            effect.position = collider.bounds.center;
            effect.addComponent<SpriteEffectComponent>();
        }

        private int getEffectsCount()
        {
            _effectsCount++;
            _effectsCount %= 100;
            return _effectsCount;
        }
    }
}
