using System;
using LudumDare40.Components.Sprites;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;

namespace LudumDare40.Components.Battle
{
    public interface IBattleEntity
    {
        void onHit(Vector2 knockback);
        void onDeath();
    }

    public class BattleComponent: Component, IUpdatable
    {
        //--------------------------------------------------
        // Battle Entity

        public IBattleEntity battleEntity;

        //--------------------------------------------------
        // HP

        private float _hp;
        public float HP
        {
            get { return _hp; }
            set { _hp = value; }
        }

        //--------------------------------------------------
        // Death animation

        private bool _dying;
        public bool Dying => _dying;

        private const float DeathDuration = 0.2f;
        private float _deathTime;

        //--------------------------------------------------
        // Immunity Duration

        public float ImmunityDuration = 0.3f;
        public float ImmunityTime { get; set; }
        public bool ForceImmunity { get; set; }

        //--------------------------------------------------
        // Hit Animation

        private const float HitAnimationDuration = 0.3f;
        private float _hitAnimation;

        //--------------------------------------------------
        // Sprites

        private SpriteMime _spriteMime;
        private AnimatedSprite _animatedSprite;

        //----------------------//------------------------//

        public override void onAddedToEntity()
        {
            _spriteMime = entity.addComponent<SpriteMime>();
            _spriteMime.color = Color.Transparent;
            _animatedSprite = entity.getComponent<AnimatedSprite>();
        }

        public void setHp(int hp)
        {
            _hp = hp;
        }

        public void onHit(CollisionResult collisionResult)
        {
            var knockback = new Vector2(Math.Sign(collisionResult.minimumTranslationVector.X), 0);
            onHit(knockback);
        }

        public void onHit(Vector2 knockback)
        {
            if (_dying) return;

            battleEntity?.onHit(knockback);
            _hitAnimation = 0.25f;
            ImmunityTime = ImmunityDuration;

            _hp--;
            if (_hp <= 0)
            {
                _animatedSprite.play("dying");
                _dying = true;
                _deathTime = DeathDuration;
                battleEntity?.onDeath();
            }
        }

        public void update()
        {
            if (ImmunityTime > 0.0f) ImmunityTime -= Time.deltaTime;

            if (_hitAnimation > 0.0f)
            {
                var n = MathHelper.Max(0, _hitAnimation - Time.deltaTime);
                _hitAnimation = n;
                if (n > 0.0f)
                {
                    var color = Color.Red * (n / HitAnimationDuration);
                    _spriteMime.setColor(color);
                }
                else
                {
                    _spriteMime.setColor(Color.Transparent);
                }
            }

            _spriteMime.setLocalOffset(_animatedSprite.localOffset);

            if (_dying && _animatedSprite.Looped)
            {
                _deathTime = MathHelper.Max(0, _deathTime - Time.deltaTime);
                if (_deathTime > 0.0f)
                {
                    var color = Color.White * (_deathTime / DeathDuration);
                    _animatedSprite.setColor(color);
                }
                else
                {
                    entity.destroy();
                }
            }
        }

        public bool isOnImmunity()
        {
            return ForceImmunity || ImmunityTime > 0.0f;
        }
    }
}
