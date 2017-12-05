using System;
using LudumDare40.Extensions;
using LudumDare40.FSM;
using LudumDare40.Managers;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using Random = Nez.Random;

namespace LudumDare40.Components.Battle.Enemies
{
    public class EnemyDroneState : State<EnemyDroneState, EnemyDroneComponent>
    {
        protected BoxCollider playerCollider => entity.playerCollider;

        public override void begin() { }

        public override void end() { }

        public override void update() { }
    }

    public class EnemyDronePatrolState : EnemyDroneState
    {
        private float _side = -1;
        private ITimer _timer;

        public override void begin()
        {
            _side = entity.patrolStartRight ? -1 : 1;
            switchSide();
            entity.sprite.play("floating");
        }

        public void switchSide()
        {
            _timer?.stop();
            _side *= -1;
            entity.forceMovement(Vector2.UnitX * _side);
            _timer = Core.schedule(entity.patrolTime, entity, t =>
            {
                switchSide();
            });
        }

        public override void update()
        {
            var po = entity.getComponent<PlatformerObject>();
            if (entity.sprite.getDirection() == 1 && po.collisionState.right)
            {
                po.velocity = -po.maxMoveSpeed * 0.8f * Vector2.UnitX;
                switchSide();
            }
            if (entity.sprite.getDirection() == -1 && po.collisionState.left)
            {
                po.velocity = po.maxMoveSpeed * 0.8f * Vector2.UnitX;
                switchSide();
            }
            if (entity.canSeeThePlayer())
            {
                _timer.stop();
                entity.forceMovement(Vector2.Zero);
                fsm.pushState(new EnemyDroneFollowState());
            }
        }

        public override void end()
        {
            _timer?.stop();
        }
    }

    public class EnemyDroneFollowState : EnemyDroneState
    {
        private float _followLimitTick;
        private float _shotCooldown;

        public override void begin()
        {
            entity.sprite.play("floating");
            _followLimitTick = 0.0f;
        }

        public override void update()
        {
            if (!entity.canSeeThePlayer())
            {
                _followLimitTick += Time.deltaTime;
                if (_followLimitTick >= 3.0f)
                {
                    fsm.resetStackTo(new EnemyDronePatrolState());
                }
            }
            var distance = entity.distanceToPlayer();
            entity.forceMovement(Vector2.UnitX * Math.Sign(distance));
            if (_shotCooldown > 0.0f)
            {
                _shotCooldown -= Time.deltaTime;
                if (entity.canSeeThePlayer())
                    entity.forceMovement(Vector2.Zero);
                return;
            }
            if (entity.canSeeThePlayer())
            {
                if (entity.dangerousStage == 1 && distance < 70)
                {
                    _shotCooldown = 1.0f;
                    fsm.pushState(new EnemyDroneShotState());
                }
                if (entity.dangerousStage == 2 && distance < 70)
                {
                    _shotCooldown = 0.8f;
                    if (Random.nextFloat() > 0.4)
                    {
                        fsm.pushState(new EnemyDroneCanonShotState());
                    }
                    else
                    {
                        fsm.pushState(new EnemyDroneShotState());
                    }
                }
            }
        }

        public override void end()
        {
            entity.forceMovement(Vector2.Zero);
        }
    }

    public class EnemyDroneShotState : EnemyDroneState
    {
        private bool _shot;

        public override void begin()
        {
            _shot = false;
            entity.sprite.play("attack");
        }

        public override void update()
        {
            if (!_shot && entity.sprite.CurrentFrame == 2)
            {
                _shot = true;
                entity.createShot(1);
            }
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new EnemyDroneFollowState());
            }
        }
    }

    public class EnemyDroneCanonShotState : EnemyDroneState
    {
        private bool _shot;

        public override void begin()
        {
            _shot = false;
            entity.sprite.play("attackStrong");
        }

        public override void update()
        {
            if (!_shot && entity.sprite.CurrentFrame == 3)
            {
                _shot = true;
                entity.createShot(2);
            }
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new EnemyDroneFollowState());
            }
        }
    }

    public class EnemyDroneHitState : EnemyDroneState
    {
        public override void begin()
        {
            AudioManager.hit.play(0.8f, 0.0f);
            entity.sprite.play("hit");
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new EnemyDroneFollowState());
            }
        }
    }

    public class EnemyDroneDyingState : EnemyDroneState
    {
        private bool _playedSe;

        public override void begin()
        {
            entity.sprite.play("dying");
        }

        public override void update()
        {
            if (!_playedSe && entity.sprite.CurrentFrame == 3)
            {
                _playedSe = true;
                AudioManager.explosion.Play();
            }
        }
    }
}
