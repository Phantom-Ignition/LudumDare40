using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LudumDare40.Components.Sprites;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;

namespace LudumDare40.Components.Battle
{
    public interface IBattleEntity
    {
        void onHit();
    }

    class BattleComponent: Component, IUpdatable
    {
        public IBattleEntity battleEntity;
        private const float ImmunityDuration = 0.3f;
        public float ImmunityTime { get; set; }

        private const float HitAnimationDuration = 0.3f;
        private float _hitAnimation;

        private SpriteMime _spriteMime;
        private AnimatedSprite _animatedSprite;

        public override void onAddedToEntity()
        {
            _spriteMime = entity.addComponent<SpriteMime>();
            _spriteMime.color = Color.Transparent;
            _animatedSprite = entity.getComponent<AnimatedSprite>();
        }

        public void onHit()
        {
            battleEntity?.onHit();
            _hitAnimation = 0.25f;
            ImmunityTime = ImmunityDuration;
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
        }
    }
}
