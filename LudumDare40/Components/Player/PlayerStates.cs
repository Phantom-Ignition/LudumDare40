using System.Runtime.Serialization;
using LudumDare40.FSM;
using LudumDare40.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace LudumDare40.Components.Player
{
    public class PlayerState : State<PlayerState, PlayerComponent>
    {
        protected InputManager _input => Core.getGlobalManager<InputManager>();

        public override void begin() { }

        public override void end() { }

        public override void handleInput()
        {
            if (isMovementAvailable())
            {
                if (entity.isOnGround() && _input.JumpButton.isPressed)
                {
                    fsm.resetStackTo(new JumpingState(true));
                }
                if (_input.UpButton.isPressed && entity.platformerObject.IsLadderTouching)
                {
                    fsm.resetStackTo(new LadderState());
                }
            }
        }

        protected bool isMovementAvailable()
        {
            return Core.getGlobalManager<InputManager>().isMovementAvailable();
        }

        public override void update() { }
    }

    public class StandState : PlayerState
    {
        public override void begin()
        {
            entity.SetAnimation(PlayerComponent.Animations.Stand);
        }
        public override void update()
        {
            if (entity.isOnGround())
            {
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

            if (isMovementAvailable() && Input.isKeyPressed(Keys.A))
            {
                fsm.pushState(new AttackStateOne());
            }
            base.handleInput();
        }
    }

    public class JumpingState : PlayerState
    {
        private float _jumpTime;
        private float _landTime;
        private bool _needJump;

        public JumpingState(bool needJump)
        {
            _needJump = needJump;
        }

        public override void begin()
        {
            entity.SetAnimation(PlayerComponent.Animations.JumpPreparation);
            if (_needJump)
                entity.Jump();
        }

        public override void update()
        {
            if (entity.isOnGround())
            {
                entity.SetAnimation(PlayerComponent.Animations.JumpLanding);
                _landTime += Time.deltaTime;
                if (_landTime >= 0.1f)
                    fsm.resetStackTo(new StandState());
                return;
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

            base.handleInput();
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
                return;
            }
            else if ((_side == -1 && !collisionState.left) || (_side == 1 && !collisionState.right))
            {
                fsm.changeState(new JumpingState(false));
            }
            base.handleInput();
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
            entity.SetAnimation(PlayerComponent.Animations.AttackOne);
        }

        public override void update()
        {
            base.update();
            if (entity.sprite.isOnCombableFrame() && Input.isKeyPressed(Keys.A))
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
    }

    public class AttackStateTwo : PlayerState
    {
        public override void begin()
        {
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
    }
}
