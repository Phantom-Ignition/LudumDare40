using LudumDare40.FSM;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System;
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
            switchSide();
            entity.sprite.play("walking");
        }

        public void switchSide()
        {
            _side *= -1;
            entity.forceMovement(Vector2.UnitX * _side);
            _timer = Core.schedule(1.5f, entity, t =>
            {
                switchSide();
            });
        }

        public override void update()
        {
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
            if (entity.canSeeThePlayer() && Math.Abs(distance) < 26)
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
                fsm.popState();
            }
        }
    }

    public class EnemyOneElectricalDischargeState : EnemyOneState
    {
        public override void begin()
        {
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
                fsm.popState();
            }
        }
    }

    public class EnemyOneHitState : EnemyOneState
    {
        public override void begin()
        {
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
        public override void begin()
        {
            entity.sprite.play("dying");
        }
    }
}
