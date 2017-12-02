using System;
using System.Collections.Generic;
using LudumDare40.Components.Sprites;
using LudumDare40.FSM;
using LudumDare40.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Tiled;

namespace LudumDare40.Components.Player
{
    public class PlayerComponent : Component, IUpdatable
    {
        //--------------------------------------------------
        // Animations

        public enum Animations
        {
            Stand,
            Walking,
            JumpPreparation,
            JumpUpwards,
            JumpFalling,
            JumpLanding,
            Hit,
            AttackOne,
            AttackTwo
        }

        private Dictionary<Animations, string> _animationMap;

        //--------------------------------------------------
        // Sprite

        public AnimatedSprite sprite;
        public AnimatedSprite coreSprite;

        //--------------------------------------------------
        // Movement Input

        private VirtualIntegerAxis _movementInput;

        //--------------------------------------------------
        // Platformer Object

        PlatformerObject _platformerObject;
        public PlatformerObject platformerObject => _platformerObject;

        //--------------------------------------------------
        // Collision State

        public TiledMapMover.CollisionState CollisionState => _platformerObject.collisionState;
        public bool ForcedGround { get; set; }

        //--------------------------------------------------
        // Velocity

        public Vector2 Velocity => _platformerObject.velocity;

        //--------------------------------------------------
        // Finite State Machine

        private FiniteStateMachine<PlayerState, PlayerComponent> _fsm;
        public FiniteStateMachine<PlayerState, PlayerComponent> FSM => _fsm;

        //--------------------------------------------------
        // Forced Movement

        private bool _forceMovement;
        private Vector2 _forceMovementVelocity;
        private bool _walljumpForcedMovement;

        //----------------------//------------------------//

        public override void initialize()
        {
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.player);

            _animationMap = new Dictionary<Animations, string>
            {
                {Animations.Stand, "stand"},
                {Animations.Walking, "walking"},
                {Animations.JumpPreparation, "jumpPreparation"},
                {Animations.JumpUpwards, "jumpUpwards"},
                {Animations.JumpFalling, "jumpFalling"},
                {Animations.JumpLanding, "jumpLanding"},
                {Animations.Hit, "jumpLanding"},
                {Animations.AttackOne, "attack1"},
                {Animations.AttackTwo, "attack2"},
            };

            var am = _animationMap;

            sprite = entity.addComponent(new AnimatedSprite(texture, am[Animations.Stand]));
            sprite.CreateAnimation(am[Animations.Stand], 0.1f);
            sprite.AddFrames(am[Animations.Stand], new List<Rectangle>()
            {
                new Rectangle(0, 0, 64, 64),
                new Rectangle(64, 0, 64, 64),
                new Rectangle(128, 0, 64, 64),
                new Rectangle(192, 0, 64, 64),
                new Rectangle(256, 0, 64, 64),
                new Rectangle(320, 0, 64, 64),
            }, new int[] { 0, 0, 0, 0, 0, 0 }, new int[] { -9, -9, -9, -9, -9, -9 });

            sprite.CreateAnimation(am[Animations.Walking], 0.1f);
            sprite.AddFrames(am[Animations.Walking], new List<Rectangle>()
            {
                new Rectangle(0, 64, 64, 64),
                new Rectangle(64, 64, 64, 64),
                new Rectangle(128, 64, 64, 64),
                new Rectangle(192, 64, 64, 64),
                new Rectangle(256, 64, 64, 64),
                new Rectangle(320, 64, 64, 64),
            }, new int[] { 0, 0, 0, 0, 0, 0 }, new int[] { -9, -9, -9, -9, -9, -9 });

            sprite.CreateAnimation(am[Animations.JumpPreparation]);
            sprite.AddFrames(am[Animations.JumpPreparation], new List<Rectangle>()
            {
                new Rectangle(0, 128, 64, 64),
            }, new int[] { 0 }, new int[] { -9 });

            sprite.CreateAnimation(am[Animations.JumpUpwards]);
            sprite.AddFrames(am[Animations.JumpUpwards], new List<Rectangle>()
            {
                new Rectangle(0, 128, 64, 64),
            }, new int[] { 0 }, new int[] { -9 });

            sprite.CreateAnimation(am[Animations.JumpFalling]);
            sprite.AddFrames(am[Animations.JumpFalling], new List<Rectangle>()
            {
                new Rectangle(0, 128, 64, 64),
            }, new int[] { 0 }, new int[] { -9 });

            sprite.CreateAnimation(am[Animations.JumpLanding]);
            sprite.AddFrames(am[Animations.JumpLanding], new List<Rectangle>()
            {
                new Rectangle(0, 128, 64, 64),
            }, new int[] { 0 }, new int[] { -9 });

            sprite.CreateAnimation(am[Animations.Hit], 0.1f);
            sprite.AddFrames(am[Animations.Hit], new List<Rectangle>()
            {
                new Rectangle(224, 32, 32, 32),
                new Rectangle(256, 32, 32, 32),
                new Rectangle(224, 32, 32, 32),
            });

            sprite.CreateAnimation(am[Animations.AttackOne], 0.08f, false);
            sprite.AddFrames(am[Animations.AttackOne], new List<Rectangle>
            {
                new Rectangle(128, 128, 64, 64),
                new Rectangle(192, 128, 64, 64),
                new Rectangle(256, 128, 64, 64),
                new Rectangle(320, 128, 64, 64),
            }, new [] { 0, 0, 0, 0 }, new[] { -12, -12, -12, -12 });
            sprite.AddAttackCollider(am[Animations.AttackOne], new List<List<Rectangle>>
            {
                new List<Rectangle>() { },
                new List<Rectangle>() { new Rectangle(0, -10, 34, 29) },
                new List<Rectangle>() { new Rectangle(4, -9, 29, 25) },
                new List<Rectangle>() { new Rectangle(5, -10, 16, 11) },
            });
            sprite.AddFramesToAttack(am[Animations.AttackOne], 1, 2, 3);
            
            sprite.CreateAnimation(am[Animations.AttackTwo], 0.08f, false);
            sprite.AddFrames(am[Animations.AttackTwo], new List<Rectangle>
            {
                new Rectangle(0, 192, 64, 64),
                new Rectangle(64, 192, 64, 64),
                new Rectangle(128, 192, 64, 64),
                new Rectangle(192, 192, 64, 64),
            }, new int[] { 0, 0, 0, 0 }, new int[] { -9, -9, -9, -9 });
            sprite.AddAttackCollider(am[Animations.AttackTwo], new List<List<Rectangle>>
            {
                new List<Rectangle>() { new Rectangle(5, -10, 16, 11) },
                new List<Rectangle>() { new Rectangle(-20, -8, 55, 24) },
                new List<Rectangle>() { new Rectangle(-22, -14, 45, 28) },
                new List<Rectangle>() { new Rectangle(-22, -14, 22, 20) },
            });
            sprite.AddFramesToAttack(am[Animations.AttackTwo], 0, 1, 2, 3);

            // Create the core sprite
            var coreTexture = entity.scene.content.Load<Texture2D>(Content.Characters.core);
            coreSprite = entity.addComponent(new AnimatedSprite(coreTexture, am[Animations.Stand]));
            coreSprite.CloneAnimationsFrom(sprite, coreTexture);

            _fsm = new FiniteStateMachine<PlayerState, PlayerComponent>(this, new StandState());

            _movementInput = new VirtualIntegerAxis();
            _movementInput.nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right));
        }

        public override void onAddedToEntity()
        {
            _platformerObject = entity.getComponent<PlatformerObject>();
        }

        public void forceMovement(Vector2 velocity, bool walljumpForcedMovement = false)
        {
            if (velocity == Vector2.Zero)
            {
                _forceMovement = false;
            }
            else
            {
                _forceMovement = true;
                _forceMovementVelocity = velocity;
            }
            _walljumpForcedMovement = walljumpForcedMovement;
        }

        public void update()
        {
            // Update FSM
            _fsm.update();

            coreSprite.spriteEffects = sprite.spriteEffects;

            var velocity = _forceMovement ? _forceMovementVelocity.X : _movementInput.value;
            if (canMove() && (velocity > 0 || velocity < 0))
            {
                var po = _platformerObject;
                var mms = po.maxMoveSpeed;
                var moveSpeed = _walljumpForcedMovement ? po.gravity * mms : po.moveSpeed;

                if (velocity != Math.Sign(po.velocity.X))
                {
                    po.velocity.X = 0;
                }
                
                po.velocity.X = (int)MathHelper.Clamp(po.velocity.X + moveSpeed * velocity * Time.deltaTime, -mms, mms);
                sprite.spriteEffects = velocity < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }
            else
            {
                _platformerObject.velocity.X = 0;
            }

            ForcedGround = false;
        }

        private bool canMove()
        {
            return Core.getGlobalManager<InputManager>().isMovementAvailable() || _forceMovement;
        }

        public bool isOnGround()
        {
            return ForcedGround || _platformerObject.collisionState.below;
        }

        public void SetAnimation(Animations animation)
        {
            var animationStr = _animationMap[animation];
            if (sprite.CurrentAnimation != animationStr)
            {
                sprite.play(animationStr);
                coreSprite.play(sprite.CurrentAnimation);
            }
        }

        public void Jump()
        {
            _platformerObject.jump();
        }
    }
}
