using LudumDare40.FSM;
using LudumDare40.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace LudumDare40.Components.Player
{
    public class PlayerState : State<PlayerState, PlayerComponent>
    {
        protected InputManager _input => Core.getGlobalManager<InputManager>();

        public override void begin() { }

        public override void end() { }

        public void handleInput()
        {
            if (isMovementAvailable())
            {
                if (entity.isOnGround() && _input.JumpButton.isPressed)
                {
                    fsm.resetStackTo(new JumpingState(true));
                }
                if (_input.UpButton.isDown && entity.platformerObject.IsLadderTouching)
                {
                    fsm.resetStackTo(new LadderState());
                }
            }
        }

        protected bool isMovementAvailable()
        {
            return Core.getGlobalManager<InputManager>().isMovementAvailable();
        }

        public override void update()
        {
            handleInput();
        }
    }

    public class StandState : PlayerState
    {
        public override void begin()
        {
            entity.SetAnimation(PlayerComponent.Animations.Stand);
        }

        public override void update()
        {
            base.update();

            if (entity.isOnGround())
            {
                if (_input.RollButton.isPressed)
                {
                    fsm.pushState(new RollingState());
                }
                if (entity.Velocity.X > 0 || entity.Velocity.X < 0)
                {
                    entity.SetAnimation(PlayerComponent.Animations.Walking);
                }
                else
                {
                    entity.SetAnimation(PlayerComponent.Animations.Stand);
                }
            }
            else
            {
                fsm.changeState(new JumpingState(false));
            }

            if (_input.TakeThrowButton.isPressed)
            {
                entity.takeThrow();
            }

            if (isMovementAvailable() && _input.AttackButton.isPressed)
            {
                fsm.pushState(new AttackStateOne());
            }
        }
    }

    public class JumpingState : PlayerState
    {
        private float _jumpTime;
        private bool _needJump;

        public JumpingState(bool needJump)
        {
            _needJump = needJump;
        }

        public override void begin()
        {
            entity.SetAnimation(PlayerComponent.Animations.JumpPreparation);
            if (_needJump)
            {
                _needJump = false;
                entity.Jump();
            }
        }

        public override void update()
        {
            base.update();

            if (entity.isOnGround())
            {
                fsm.resetStackTo(new StandState());
            }
            else if (entity.platformerObject.collisionState.left)
            {
                fsm.changeState(new WallJumpState(-1));
            }
            else if (entity.platformerObject.collisionState.right)
            {
                fsm.changeState(new WallJumpState(1));
            }

            if (isMovementAvailable() && Input.isKeyPressed(Keys.A))
            {
                fsm.pushState(new AttackStateOne());
            }

            _jumpTime += Time.deltaTime;

            if (_jumpTime >= 0.05f)
            {
                if (entity.Velocity.Y < 0)
                {
                    entity.SetAnimation(PlayerComponent.Animations.JumpUpwards);
                }
                else if (entity.Velocity.Y > 0)
                {
                    entity.SetAnimation(PlayerComponent.Animations.JumpFalling);
                }
            }
        }
    }

    public class RollingState : PlayerState
    {
        private int[] _immunityFrames;

        public override void begin()
        {
            _immunityFrames = new[] {1, 2};
            entity.SetAnimation(PlayerComponent.Animations.Rolling);
            entity.forceMovement(Vector2.UnitX * (entity.sprite.spriteEffects == SpriteEffects.FlipHorizontally ? -1 : 1));
            entity.isRolling = true;
            Core.schedule(0.35f, entity, t =>
            {
                fsm.popState();
            });
        }

        public override void update()
        {
            entity.battleComponent.ForceImmunity = _immunityFrames.contains(entity.sprite.CurrentFrame);
        }

        public override void end()
        {
            entity.isRolling = false;
            entity.forceMovement(Vector2.Zero);
        }
    }

    public class WallJumpState : PlayerState
    {
        private int _side;

        public WallJumpState(int side)
        {
            _side = side;
        }

        public override void begin()
        {
            entity.SetAnimation(PlayerComponent.Animations.Stand);
            entity.platformerObject.grabbingWall = true;
        }

        public override void update()
        {
            base.update();

            var collisionState = entity.platformerObject.collisionState;
            if (entity.isOnGround())
            {
                fsm.resetStackTo(new StandState());
            }
            else if (_input.JumpButton.isPressed)
            {
                fsm.changeState(new JumpingState(true));
                entity.forceMovement(Vector2.UnitX * _side * -1, true);
                Core.schedule(0.2f, entity, t =>
                {
                    var self = t.context as PlayerComponent;
                    self.forceMovement(Vector2.Zero);
                });
            }
            else if ((_side == -1 && !collisionState.left) || (_side == 1 && !collisionState.right))
            {
                fsm.changeState(new JumpingState(false));
            }
        }

        public override void end()
        {
            entity.platformerObject.grabbingWall = false;
        }
    }

    public class LadderState : PlayerState
    {
        public override void begin()
        {
            entity.SetAnimation(PlayerComponent.Animations.Stand);
            entity.platformerObject.enterOnLadder();
        }

        public override void update()
        {
            if (entity.isOnGround() || !entity.platformerObject.IsLadderTouching)
            {
                fsm.resetStackTo(new StandState());
            }
            else if (_input.JumpButton.isPressed)
            {
                fsm.changeState(new JumpingState(true));
            }
            else if (_input.DownButton.isDown)
            {
                entity.platformerObject.ladderVelocityDown();
            }
            else if (_input.UpButton.isDown)
            {
                entity.platformerObject.ladderVelocityUp();
            }
        }

        public override void end()
        {
            entity.platformerObject.gabbingLadder = false;
        }
    }

    public class AttackStateOne : PlayerState
    {
        private bool _changeToAttack;

        public override void begin()
        {
            _input.IsLocked = true;
            entity.SetAnimation(PlayerComponent.Animations.AttackOne);
        }

        public override void update()
        {
            base.update();
            if (entity.sprite.isOnCombableFrame() && _input.AttackButton.isPressed)
            {
                _changeToAttack = true;
            }
            if (entity.sprite.Looped)
            {
                if (_changeToAttack)
                {
                    fsm.changeState(new AttackStateTwo());
                }
                else
                {
                    if (entity.isOnGround())
                    {
                        fsm.resetStackTo(new StandState());
                    }
                    else
                    {
                        fsm.popState();
                    }
                }
            }
        }

        public override void end()
        {
            _input.IsLocked = false;
        }
    }

    public class AttackStateTwo : PlayerState
    {
        public override void begin()
        {
            _input.IsLocked = true;
            entity.SetAnimation(PlayerComponent.Animations.AttackTwo);
        }

        public override void update()
        {
            base.update();
            if (entity.sprite.Looped)
            {
                if (entity.isOnGround())
                {
                    fsm.resetStackTo(new StandState());
                }
                else
                {
                    fsm.popState();
                }
            }
        }

        public override void end()
        {
            _input.IsLocked = false;
        }
    }
}
