using LudumDare40.Components.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System.Collections.Generic;
using LudumDare40.FSM;

namespace LudumDare40.Components.Battle.Enemies
{
    public class EnemyDroneComponent : EnemyComponent
    {
        //--------------------------------------------------
        // Finite State Machine

        private FiniteStateMachine<EnemyDroneState, EnemyDroneComponent> _fsm;
        public FiniteStateMachine<EnemyDroneState, EnemyDroneComponent> FSM => _fsm;

        //----------------------//------------------------//

        public override void initialize()
        {
            base.initialize();

            // sprite init
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.enemyDrone);
            sprite = entity.addComponent(new AnimatedSprite(texture, "floating"));
            sprite.CreateAnimation("floating", 0.25f);
            sprite.AddFrames("floating", new List<Rectangle>
            {
                new Rectangle(0, 0, 64, 64),
                new Rectangle(64, 0, 64, 64),
                new Rectangle(128, 0, 64, 64),
                new Rectangle(192, 0, 64, 64),
                new Rectangle(256, 0, 64, 64),
                new Rectangle(320, 0, 64, 64),
            }, new[] { 0, 0, 0, 0, 0, 0 }, new[] { -40, -40, -40, -40, -40, -40 });

            sprite.CreateAnimation("attack", 0.1f);
            sprite.AddFrames("attack", new List<Rectangle>
            {
                new Rectangle(0, 64, 64, 64),
                new Rectangle(64, 64, 64, 64),
                new Rectangle(128, 64, 64, 64),
                new Rectangle(192, 64, 64, 64),
                new Rectangle(256, 64, 64, 64),
                new Rectangle(320, 64, 64, 64),
            }, new[] { 0, 0, 0, 0, 0, 0 }, new[] { -40, -40, -40, -40, -40, -40 });

            sprite.CreateAnimation("hit", 0.1f);
            sprite.AddFrames("hit", new List<Rectangle>
            {
                new Rectangle(0, 128, 64, 64),
                new Rectangle(64, 128, 64, 64),
                new Rectangle(128, 128, 64, 64),
                new Rectangle(192, 128, 64, 64),
            }, new[] { 0, 0, 0, 0 }, new[] { -40, -40, -40, -40 });

            sprite.CreateAnimation("dying", 0.1f);
            sprite.AddFrames("dying", new List<Rectangle>
            {
                new Rectangle(0, 192, 64, 64),
                new Rectangle(64, 192, 64, 64),
                new Rectangle(128, 192, 64, 64),
                new Rectangle(192, 192, 64, 64),
                new Rectangle(256, 192, 64, 64),
                new Rectangle(320, 192, 64, 64),
                new Rectangle(0, 256, 64, 64),
            }, new[] { 0, 0, 0, 0, 0, 0, 0 }, new[] { -40, -40, -40, -40, -40, -40, -40 });

            // collisor init
            entity.addComponent(new BoxCollider(-12f, -32f, 32f, 35f));

            // FSM
            _fsm = new FiniteStateMachine<EnemyDroneState, EnemyDroneComponent>(this, new EnemyDronePatrol());

            // View range
            areaOfSight = entity.addComponent(new AreaOfSightCollider(-24, -12, 92, 32));
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            // Change move speed
            platformerObject.maxMoveSpeed = 60;
            platformerObject.moveSpeed = 60;
        }

        public override void onHit(Vector2 knockback)
        {
            if (dangerousStage <= 1)
            {
                base.onHit(knockback);
                FSM.changeState(new EnemyDroneHitState());
            }
        }

        public override void onDeath()
        {
            FSM.changeState(new EnemyDroneDyingState());
        }

        public override void update()
        {
            _fsm.update();
            base.update();
        }
    }
}
