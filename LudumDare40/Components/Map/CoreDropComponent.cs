using LudumDare40.Scenes;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System;

namespace LudumDare40.Components.Map
{
    class CoreDropComponent: Component, IUpdatable
    {
        private Sprite sprite;
        private float _floatingTick;

        public override void onAddedToEntity()
        {
            sprite = entity.getComponent<Sprite>();
            entity.setTag(SceneMap.COREDROPS);
        }

        public void update()
        {
            _floatingTick += (float)Math.PI * Time.deltaTime;
            _floatingTick %= (float)Math.PI * 2;
            var offset = new Vector2(0, -1 + 4 * (float)Math.Sin(_floatingTick));
            sprite.setLocalOffset(offset);
        }
    }
}
