using System.Collections.Generic;
using LudumDare40.Components.Battle;
using LudumDare40.Components.Sprites;
using LudumDare40.Extensions;
using LudumDare40.Managers;
using LudumDare40.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace LudumDare40.Components.Map
{
    public class ReactorComponent : Component
    {
        public AnimatedSprite sprite;

        private bool _isActivated;
        public bool isActivated => _isActivated;

        public override void initialize()
        {
            var texture = entity.scene.content.Load<Texture2D>(Content.Misc.reactor);

            sprite = entity.addComponent(new AnimatedSprite(texture, "off") {renderLayer = SceneMap.MISC_RENDER_LAYER});
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

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            entity.setTag(SceneMap.REACTORS);
        }

        public void setActivated()
        {
            _isActivated = true;
            sprite.play("activated");

            AudioManager.equip.Play(0.4f);

            var enemies = entity.scene.findEntitiesWithTag(SceneMap.ENEMIES);
            foreach (var enemy in enemies)
            {
                enemy.getComponent<EnemyComponent>().increaseDangerousStage();
            }

            Core.getGlobalManager<PlayerManager>().CoresCollected++;
        }
    }
}
