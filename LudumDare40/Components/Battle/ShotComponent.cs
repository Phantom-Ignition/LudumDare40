using System;
using System.Collections.Generic;
using System.Linq;
using LudumDare40.Components.Sprites;
using LudumDare40.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.ECS.Components.Physics.Colliders;

namespace LudumDare40.Components.Battle
{
    class ShotComponent : Component, IUpdatable
    {
        //--------------------------------------------------
        // Sprite

        public AnimatedSprite sprite;

        //--------------------------------------------------
        // Direction, Speed and Type

        private int _direction;
        private float _speed;
        private int _type;

        //----------------------//------------------------//

        public ShotComponent(int direction, int type)
        {
            _direction = direction;
            _type = type;
        }

        public override void initialize()
        {
            var file = _type == 1 ? Content.Misc.bullet : Content.Misc.cannon;
            var texture = entity.scene.content.Load<Texture2D>(file);
            sprite = entity.addComponent(new AnimatedSprite(texture, "default"));
            sprite.CreateAnimation("default", 0.2f);
            addFramesBasedOnType();

            entity.addComponent(new BoxCollider(-3, -3, 6, 6));

            _speed = _type == 1 ? 100f : 300f;

            entity.transform.rotationDegrees = 19;
            if (_direction < 0)
            {
                sprite.spriteEffects = SpriteEffects.FlipHorizontally;
                entity.transform.rotationDegrees = -19;
            }
        }

        private void addFramesBasedOnType()
        {
            if (_type == 1)
            {
                sprite.AddFrames("default", new List<Rectangle>
                {
                    new Rectangle(0, 0, 10, 10),
                    new Rectangle(10, 0, 10, 10),
                });
            }
            else
            {
                sprite.AddFrames("default", new List<Rectangle>
                {
                    new Rectangle(0, 0, 22, 22),
                    new Rectangle(22, 0, 22, 22),
                });
            }
        }

        public override void onAddedToEntity()
        {
            entity.setTag(SceneMap.SHOTS);
        }

        public void update()
        {
            var velx = (float)Math.Cos(entity.transform.rotation) * _speed * _direction * Time.deltaTime;
            var vely = (float)Math.Sin(entity.transform.rotation * _direction) * _speed * Time.deltaTime + 0.4f;
            var vel = new Vector2(velx, vely);
            entity.setPosition(entity.position + vel);
        }
    }
}
