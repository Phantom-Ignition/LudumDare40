using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LudumDare40.FSM;
using Microsoft.Xna.Framework;
using Nez;

namespace LudumDare40.Components.Battle.Enemies
{
    public class EnemyDroneState : State<EnemyDroneState, EnemyDroneComponent>
    {
        protected BoxCollider playerCollider => entity.playerCollider;

        public override void begin() { }

        public override void end() { }

        public override void update() { }
    }

    public class EnemyDronePatrol : EnemyDroneState
    {
        private float _side;
        private ITimer _timer;

        public override void begin()
        {
            _side = -1;
            switchSide();
            entity.sprite.play("floating");
        }

        public void switchSide()
        {
            _side *= -1;
            entity.forceMovement(Vector2.UnitX * _side);
            _timer = Core.schedule(3f, entity, t =>
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
                fsm.pushState(new EnemyDroneFollowState());
            }
        }
    }

    public class EnemyDroneFollowState : EnemyDroneState
    {
        public override void begin()
        {
            entity.sprite.play("floating");
        }

        public override void update()
        {

        }

        public override void end()
        {
            entity.forceMovement(Vector2.Zero);
        }
    }

    public class EnemyDroneHitState : EnemyDroneState
    {
        public override void begin()
        {
            entity.sprite.play("hit");
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new EnemyDronePatrol());
            }
        }
    }

    public class EnemyDroneDyingState : EnemyDroneState
    {
        public override void begin()
        {
            entity.sprite.play("dying");
        }
    }
}
