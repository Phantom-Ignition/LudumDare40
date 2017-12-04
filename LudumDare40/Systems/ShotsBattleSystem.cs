using LudumDare40.Components.Battle;
using LudumDare40.Components.Map;
using LudumDare40.Components.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.ECS.Components.Physics.Colliders;
using System;
using System.Collections.Generic;

namespace LudumDare40.Systems
{
    class ShotsBattleSystem : EntityProcessingSystem
    {
        private readonly BattleComponent _playerBattler;
        private readonly BoxCollider _playerCollider;

        private Texture2D _bullectEffectTexture;
        private int _effectsCount;

        public ShotsBattleSystem(Entity player) : base(new Matcher().all(typeof(ShotComponent), typeof(BoxCollider)))
        {
            _playerBattler = player.getComponent<BattleComponent>();
            _playerCollider = player.getComponent<BoxCollider>();

            _bullectEffectTexture = player.scene.content.Load<Texture2D>(Content.Effects.bulletEffect);
        }

        public override void process(Entity entity)
        {
            CollisionResult collisionResult;
            var collider = entity.getComponent<Collider>();
            if (collider.collidesWithAnyOfType<MapBoxCollider>(out collisionResult))
            {
                createBulletEffect(entity.getComponent<ShotComponent>(), collisionResult.normal);
                entity.destroy();
            }

            if (_playerBattler.isOnImmunity()) return;
            if (collider.collidesWith(_playerCollider, out collisionResult))
            {
                _playerBattler.onHit(collisionResult);
            }
        }
        
        private void createBulletEffect(ShotComponent bullet, Vector2 normal)
        {
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

        private int getEffectsCount()
        {
            _effectsCount++;
            _effectsCount %= 100;
            return _effectsCount;
        }
    }
}
