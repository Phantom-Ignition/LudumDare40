using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LudumDare40.Components.Player;
using LudumDare40.Components.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace LudumDare40.Components.Battle
{
    class EnemyComponent : Component, IBattleEntity
    {
        public AnimatedSprite sprite;

        public override void initialize()
        {
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.elliot);
            sprite = entity.addComponent(new AnimatedSprite(texture, "stand"));
            sprite.CreateAnimation("stand", 0.25f);
            sprite.AddFrames("stand", new List<Rectangle>()
            {
                new Rectangle(0, 0, 64, 64),
                new Rectangle(64, 0, 64, 64),
                new Rectangle(128, 0, 64, 64),
                new Rectangle(192, 0, 64, 64),
            }, new int[] { 0, 0, 0, 0 }, new int[] { -12, -12, -12, -12 });
        }

        public override void onAddedToEntity()
        {
            var battleComponent = entity.getComponent<BattleComponent>();
            battleComponent.battleEntity = this;
        }

        public void onHit()
        {
            
        }
    }
}
