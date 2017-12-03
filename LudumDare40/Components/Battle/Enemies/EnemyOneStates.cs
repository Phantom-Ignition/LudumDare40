using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LudumDare40.Components.Player;
using LudumDare40.FSM;
using LudumDare40.Managers;
using Microsoft.Xna.Framework;
using Nez;

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
        private float _side;

        public override void begin()
        {
            _side = -1;
            switchSide();
            entity.sprite.play("walking");
        }

        public void switchSide()
        {
            _side *= -1;
            entity.forceMovement(Vector2.UnitX * _side);
            Core.schedule(3f, entity, t =>
            {
                switchSide();
            });
        }

        public override void update()
        {
            Console.WriteLine(entity.sprite.CurrentFrame);
            //entity.forceMovement(Vector2.UnitX * _side);
        }
    }

    public class EnemyOneFollowState : EnemyOneState
    {

    }
}
