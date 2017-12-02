using System.Collections.Generic;
using LudumDare40.Components.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace LudumDare40.Components.Map
{
    public class ReactorComponent : Component
    {
        public AnimatedSprite sprite;

        public override void initialize()
        {
            var texture = entity.scene.content.Load<Texture2D>(Content.Misc.reactor);

            sprite = entity.addComponent(new AnimatedSprite(texture, "off"));
            sprite.CreateAnimation("off", 0.1f, false);
            sprite.AddFrames("off", new List<Rectangle>
            {
                new Rectangle(0, 0, 32, 32),
            });

            sprite.CreateAnimation("activated", 0.3f);
            sprite.AddFrames("activated", new List<Rectangle>
            {
                new Rectangle(32, 0, 32, 32),
                new Rectangle(64, 0, 32, 32),
                new Rectangle(96, 0, 32, 32),
            });
        }
    }
}
