using LudumDare40.FSM;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System;
using LudumDare40.Extensions;
using LudumDare40.Managers;
using Random = Nez.Random;

namespace LudumDare40.Components.Battle.Enemies
{
    public class EnemyOneState : State<EnemyOneState, EnemyOneComponent>
    {
        protected BoxCollider playerCollider => entity.playerCollider;

        public override void begin() { }

        public override void end() { }

        public override void update() { }
    }

    public class EnemyOnePatrolState : EnemyOneState
    {
        private float _side = -1;
        private ITimer _timer;

        public override void begin()
        {
            _side = entity.patrolStartRight ? -1 : 1;
            switchSide();
            entity.sprite.play("walking");
        }

        public void switchSide()
        {
            _timer?.stop();
            _side *= -1;
            entity.forceMovement(Vector2.UnitX * _side);
            _timer = Core.schedule(1.5f, entity, t =>
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
                fsm.pushState(new EnemyOneFollowState());
            }
        }
    }

    public class EnemyOneFollowState : EnemyOneState
    {
        private float _electricalDischargeCooldown;
        private float _followLimitTick;

        public override void begin()
        {
            entity.sprite.play("walking");
            _followLimitTick = 0.0f;
        }

        public override void update()
        {
            if (!entity.canSeeThePlayer())
            {
                _followLimitTick += Time.deltaTime;
                if (_followLimitTick >= 3.0f)
                {
                    fsm.resetStackTo(new EnemyOnePatrolState());
                }
            }
            if (_electricalDischargeCooldown > 0.0f)
            {
                _electricalDischargeCooldown = Math.Max(0, _electricalDischargeCooldown - Time.deltaTime);
            }
            var distance = entity.distanceToPlayer();
            entity.forceMovement(Vector2.UnitX * Math.Sign(distance));
            if (entity.canSeeThePlayer() && Math.Abs(distance) < 24)
            {
                fsm.pushState(new EnemyOnePunchState());
            }
            else if (entity.dangerousStage > 1 && _electricalDischargeCooldown <= 0.0f && entity.canSeeThePlayer() && Math.Abs(distance) < 40)
            {
                _electricalDischargeCooldown = 0.8f;
                var ran = Random.nextFloat();
                if (ran > 0.4)
                    fsm.pushState(new EnemyOneElectricalDischargeState());
            }
        }

        public override void end()
        {
            entity.forceMovement(Vector2.Zero);
        }
    }

    public class EnemyOnePunchState : EnemyOneState
    {
        public override void begin()
        {
            var distanceToPlayer = entity.distanceToPlayer();
            var sign = Math.Sign(distanceToPlayer);
            entity.sprite.spriteEffects = sign < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            entity.forceMovement(Vector2.Zero);
            entity.sprite.play("punch");
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new EnemyOneFollowState());
            }
        }
    }

    public class EnemyOneElectricalDischargeState : EnemyOneState
    {
        public override void begin()
        {
            AudioManager.electric.Play(1.0f);
            var distanceToPlayer = entity.distanceToPlayer();
            var sign = Math.Sign(distanceToPlayer);
            entity.sprite.spriteEffects = sign < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            entity.forceMovement(Vector2.Zero);
            entity.sprite.play("electricalDischarge");
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new EnemyOneFollowState());
            }
        }
    }

    public class EnemyOneHitState : EnemyOneState
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
                fsm.resetStackTo(new EnemyOneFollowState());
            }
        }
    }

    public class EnemyOneDyingState : EnemyOneState
    {
        private bool _playedSe;

        public override void begin()
        {
            entity.sprite.play("dying");
        }

        public override void update()
        {
            if (!_playedSe && entity.sprite.CurrentFrame == 4)
            {
                _playedSe = true;
                AudioManager.explosion.Play();
            }
        }
    }
}
