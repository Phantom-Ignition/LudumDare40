using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LudumDare40.Components.Sprites;
using LudumDare40.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace LudumDare40.Components.Battle
{
    class MissileComponent : Component, IUpdatable
    {
        //--------------------------------------------------
        // Sprite

        public AnimatedSprite sprite;

        //--------------------------------------------------
        // Direction, Speed and Type

        private float _rotation;
        private float _speed;

        private bool yTime;
        private Vector2 _initialPosition;
        private Vector2 _accumulatedPosition;

        //----------------------//------------------------//

        public MissileComponent(float rotation)
        {
            _rotation = rotation;
        }

        public override void initialize()
        {
            var texture = entity.scene.content.Load<Texture2D>(Content.Misc.missile);
            sprite = entity.addComponent(new AnimatedSprite(texture, "default"));
            sprite.CreateAnimation("default", 0.2f);
            sprite.AddFrames("default", new List<Rectangle>
            {
                new Rectangle(0, 0, 32, 32),
                new Rectangle(32, 0, 32, 32),
            });

            entity.addComponent(new BoxCollider(-3, -3, 6, 6));

            _speed = -200f;

            entity.transform.rotationDegrees = _rotation;
        }

        public void setInitialPosition(Vector2 pos)
        {
            _initialPosition = pos;
        }

        public override void onAddedToEntity()
        {
            entity.setTag(SceneMap.SHOTS);
        }

        public void update()
        {
            var velx = (float)Math.Cos(entity.transform.rotation) * _speed * Time.deltaTime;
            var vely = (float)Math.Sin(entity.transform.rotation) * _speed * Time.deltaTime;
            _accumulatedPosition += new Vector2(velx, vely);
            entity.setPosition(_initialPosition + _accumulatedPosition);
        }
    }
}
