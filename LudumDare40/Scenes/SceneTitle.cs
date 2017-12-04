using System;
using LudumDare40.Managers;
using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Nez.Sprites;
using Random = Nez.Random;
using LudumDare40.Extensions;

namespace LudumDare40.Scenes
{
    class SceneTitle : Scene
    {
        private const float Tau = 6.28318530718f;

        //--------------------------------------------------
        // Menu

        private Vector2[] _cursorPositions;
        private int _index;

        private Sprite _cursorSprite;

        private PixelGlitchPostProcessor _pixelGlitchPostProcessor;

        private BloomSettings settings;

        public float _blurAmount;

        private BloomPostProcessor _bloomPostProcessor;
        private float _bloomPulseTick;

        private ITimer _glitchTimer;
        private float _glitchCooldown;
        
        private bool _fading;

        //----------------------//------------------------//

        public override void initialize()
        {
            addRenderer(new DefaultRenderer());
            clearColor = new Color(58, 61, 101);
            createMenu();

            _bloomPostProcessor = addPostProcessor(new BloomPostProcessor(2));
            _pixelGlitchPostProcessor = addPostProcessor(new PixelGlitchPostProcessor(1));
            _pixelGlitchPostProcessor.horizontalOffset = 0;
            _blurAmount = 1;

            _glitchCooldown = 3.0f;

            MediaPlayer.Play(AudioManager.mystOnTheMoor);
        }

        private void createMenu()
        {
            _cursorPositions = new[]
            {
                new Vector2(8, 157),
                new Vector2(8, 200),
            };

            createEntity("background")
                .addComponent(new Sprite(content.Load<Texture2D>(Content.Title.background)))
                .setOriginNormalized(Vector2.Zero);

            createEntity("logo")
                .addComponent(new Sprite(content.Load<Texture2D>(Content.Title.menu_logo)))
                .setOriginNormalized(Vector2.Zero)
                .transform.position = new Vector2(22, 35);

            createEntity("menu-options")
                .addComponent(new Sprite(content.Load<Texture2D>(Content.Title.menu_opt)))
                .setOriginNormalized(Vector2.Zero)
                .transform.position = new Vector2(22, 157);

            var cursor = createEntity("cursor");
            _cursorSprite = cursor.addComponent(new Sprite(content.Load<Texture2D>(Content.Title.menu_cursor)));
            _cursorSprite.originNormalized = Vector2.Zero;
            _cursorSprite.transform.position = _cursorPositions[0];
        }

        public override void update()
        {
            base.update();
            
            // Glitch
            if (_glitchCooldown <= 0.0f)
            {
                _pixelGlitchPostProcessor.horizontalOffset = 3 + Random.nextInt(10);
                _glitchTimer = Core.schedule(Math.Min(0.01f + Random.nextFloat()/10, 0.1f), t =>
                {
                    _pixelGlitchPostProcessor.horizontalOffset = 0;
                });
                _glitchCooldown = 2 + Random.nextInt(3);
            }
            else
            {
                _glitchCooldown -= Time.deltaTime;
            }

            // Bloom
            _bloomPulseTick += Time.deltaTime / 10;
            settings = new BloomSettings(0.5f, PulseTime(_bloomPulseTick), 0.9f, 1, 1, 1);
            _bloomPostProcessor.setBloomSettings(settings);

            // Menu
            var input = Core.getGlobalManager<InputManager>();
            var lastIndex = _index;
            
            if (!_fading && input.UpButton.isPressed)
            {
                _index = _index - 1 < 0 ? _cursorPositions.Length - 1 : _index - 1;
            }
            if (!_fading && input.DownButton.isPressed)
            {
                _index = _index + 1 >= _cursorPositions.Length ? 0 : _index + 1;
            }

            if (lastIndex != _index)
            {
                AudioManager.switchSe.Play(0.5f);
                _cursorSprite.transform.position = _cursorPositions[_index];
            }

            if (!_fading && input.SelectButton.isPressed)
            {
                AudioManager.select.Play(0.5f);
                _fading = true;
                if (_index == 0)
                {
                    Core.startSceneTransition(new FadeTransition(() => new SceneMap()) { fadeOutDuration = 3.0f });
                }
                else
                {
                    Core.exit();
                }
            }
        }

        public override void unload()
        {
            base.unload();
            _glitchTimer?.stop();
        }

        public float PulseTime(float time)
        {
            return Math.Min(1.2f, 1f + Mathf.sin(time * Tau * 2) * 0.5f);
        }
    }
}
