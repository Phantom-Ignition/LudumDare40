using LudumDare40.Extensions;
using LudumDare40.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;

namespace LudumDare40.Scenes
{
    class SceneCredits : Scene
    {
        private bool _onTransition;

        public override void initialize()
        {
            addRenderer(new DefaultRenderer());
            clearColor = new Color(33, 33, 39);

            createEntity("credits-sprite")
                .addComponent(new Sprite(content.Load<Texture2D>(Content.Misc.credits)))
                .transform.position = virtualSize.ToVector2() / 2 + 5 * Vector2.UnitY;
        }

        public override void update()
        {
            base.update();
            if (_onTransition) return;

            if (Core.getGlobalManager<InputManager>().SelectButton.isPressed)
            {
                _onTransition = true;
                AudioManager.select.Play(0.6f);
                Core.startSceneTransition(new FadeTransition(() => new SceneTitle()) { fadeOutDuration = 1.5f });
            }
        }
    }
}
