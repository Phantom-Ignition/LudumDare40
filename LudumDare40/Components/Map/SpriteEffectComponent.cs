using LudumDare40.Components.Sprites;
using Nez;

namespace LudumDare40.Components.Map
{
    class SpriteEffectComponent : Component, IUpdatable
    {
        private AnimatedSprite _sprite;

        public override void onAddedToEntity()
        {
            _sprite = entity.getComponent<AnimatedSprite>();
        }

        public void update()
        {
            if (_sprite.Looped)
                entity.destroy();
        }
    }
}
