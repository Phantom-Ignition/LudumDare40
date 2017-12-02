using System.Collections.Generic;
using LudumDare40.Components.Battle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace LudumDare40.Components.Sprites
{
    public class AnimatedSprite<T> : Sprite, IUpdatable
    {
        //--------------------------------------------------
        // Frames stuff

        private int _currentFrame;
        public int CurrentFrame => _currentFrame;

        private T _currentFrameList;
        public T CurrentAnimation => _currentFrameList;

        private bool _looped;
        public bool Looped => _looped;

        //--------------------------------------------------
        // Animations

        private Dictionary<T, FramesList> _animations;
        private float _delayTick;

        //----------------------//------------------------//

        public AnimatedSprite(Texture2D texture, T initialFrame) : base(texture)
        {
            _currentFrame = 0;
            _currentFrameList = initialFrame;
            _delayTick = 0;
            _animations = new Dictionary<T, FramesList>();
            _looped = false;
        }
        
        public void CreateAnimation(T animation, float delay)
        {
            _animations[animation] = new FramesList(delay);
        }

        public void CreateAnimation(T animation, float delay, bool reset)
        {
            _animations[animation] = new FramesList(delay);
            _animations[animation].Reset = reset;
        }

        public void CreateAnimation(T animation)
        {
            _animations[animation] = new FramesList(0);
        }

        public void ResetCurrentAnimation()
        {
            _currentFrame = 0;
            _looped = false;
            _delayTick = 0;
        }

        public void AddFrames(T animation, List<Rectangle> frames, int[] offsetX, int[] offsetY)
        {
            for (var i = 0; i < frames.Count; i++)
            {
                var frameSubtexture = new Subtexture(subtexture.texture2D, frames[i]);
                _animations[animation].Frames.Add(new FrameInfo(frameSubtexture, offsetX[i], offsetY[i]));
            }
        }

        public void AddAttackCollider(T animation, List<List<Rectangle>> rectangleFrames)
        {
            for (var i = 0; i < rectangleFrames.Count; i++)
            {
                for (var j = 0; j < rectangleFrames[i].Count; j++)
                {
                    var collider = new AttackCollider(rectangleFrames[i][j].X, rectangleFrames[i][j].Y, rectangleFrames[i][j].Width, rectangleFrames[i][j].Height);
                    entity.addComponent(collider);
                    _animations[animation].Frames[i].AttackColliders.Add(collider);
                }
            }
        }

        public void AddFramesToAttack(T animation, params int[] frames)
        {
            for (var i = 0; i < frames.Length; i++)
                _animations[animation].FramesToAttack.Add(frames[i]);
        }

        public void AddFrames(T animation, List<Rectangle> frames)
        {
            var offsetX = new int[frames.Count];
            var offsetY = new int[frames.Count];
            AddFrames(animation, frames, offsetX, offsetY);
        }

        void IUpdatable.update()
        {
            foreach (var frame in _animations[_currentFrameList].Frames)
            {
                foreach (var collider in frame.AttackColliders)
                {
                    var offsetX = 0f;
                    if (spriteEffects == SpriteEffects.FlipHorizontally)
                        offsetX = -2 * collider.X;
                    collider.ApplyOffset(offsetX, 0);
                    //collider.transform.setPosition(transform.position);
                }
            }

            if (_animations[_currentFrameList].Loop)
            {
                var currentAnimation = _animations[_currentFrameList];
                _delayTick += Time.deltaTime;
                if (_delayTick > currentAnimation.Delay)
                {
                    _delayTick -= currentAnimation.Delay;
                    _currentFrame++;
                    if (_currentFrame == currentAnimation.Frames.Count)
                    {
                        if (!currentAnimation.Reset)
                        {
                            _currentFrame--;
                            currentAnimation.Loop = false;
                        }
                        else _currentFrame = 0;
                        if (!_looped) _looped = true;
                    }
                }
                var currentFrame = currentAnimation.Frames[_currentFrame];
                var rsubtexture = currentFrame.Subtexture;
                setSubtexture(rsubtexture);
                _localOffset = new Vector2(currentFrame.OffsetX, currentFrame.OffsetY);
            }
        }

        public void play(T animation)
        {
            _currentFrame = 0;
            _delayTick = 0;
            _currentFrameList = animation;
            _looped = false;
            if (!_animations[_currentFrameList].Reset)
            {
                _animations[_currentFrameList].Loop = true;
            }
        }

        public override void debugRender(Graphics graphics)
        {
            base.debugRender(graphics);

            foreach (var frame in _animations[_currentFrameList].Frames)
            {
                foreach (var collider in frame.AttackColliders)
                {
                    collider.debugRender(graphics, true);
                }
            }
        }
    }
}
