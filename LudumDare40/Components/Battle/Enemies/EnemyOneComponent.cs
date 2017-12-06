using LudumDare40.Components.Sprites;
using LudumDare40.FSM;
using LudumDare40.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System.Collections.Generic;
using LudumDare40.Managers;

namespace LudumDare40.Components.Battle.Enemies
{
    public class EnemyOneComponent : EnemyComponent
    {
        //--------------------------------------------------
        // Finite State Machine

        private FiniteStateMachine<EnemyOneState, EnemyOneComponent> _fsm;
        public FiniteStateMachine<EnemyOneState, EnemyOneComponent> FSM => _fsm;

        //--------------------------------------------------
        // Skills

        public float electricalDischargeCooldown;

        //----------------------//------------------------//

        public EnemyOneComponent(bool patrolStartRight) : base(patrolStartRight)
        {
        }

        public override void initialize()
        {
            base.initialize();

            // Init sprite
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.enemyOne);
            sprite = entity.addComponent(new AnimatedSprite(texture, "stand"));
            sprite.CreateAnimation("stand", 0.25f);
            sprite.AddFrames("stand", new List<Rectangle>
            {
                new Rectangle(0, 0, 67, 62),
                new Rectangle(67, 0, 67, 62),
                new Rectangle(134, 0, 67, 62),
                new Rectangle(201, 0, 67, 62),
                new Rectangle(268, 0, 67, 62),
                new Rectangle(335, 0, 67, 62),
            }, new[] { 0, 0, 0, 0, 0, 0 }, new[] { -4, -4, -4, -4, -4, -4 });
            
            sprite.CreateAnimation("walking", 0.15f);
            sprite.AddFrames("walking", new List<Rectangle>
            {
                new Rectangle(335, 124, 67, 62),
                new Rectangle(402, 124, 67, 62),
                new Rectangle(0, 186, 67, 62),
                new Rectangle(67, 186, 67, 62),
                new Rectangle(134, 186, 67, 62),
                new Rectangle(201, 186, 67, 62),
            }, new[] { 0, 0, 0, 0, 0, 0 }, new[] { -4, -4, -4, -4, -4, -4 });

            sprite.CreateAnimation("punch", 0.1f, false);
            sprite.AddFrames("punch", new List<Rectangle>
            {
                new Rectangle(402, 0, 67, 62),
                new Rectangle(0, 62, 67, 62),
                new Rectangle(67, 62, 67, 62),
                new Rectangle(134, 62, 67, 62),
                new Rectangle(201, 62, 67, 62),
                new Rectangle(268, 62, 67, 62),
            }, new[] { 0, 0, 0, 0, 0, 0 }, new[] { -4, -4, -4, -4, -4, -4 });
            sprite.AddAttackCollider("punch", new List<List<Rectangle>>
            {
                new List<Rectangle> { new Rectangle() },
                new List<Rectangle> { new Rectangle() },
                new List<Rectangle> { new Rectangle() },
                new List<Rectangle> { new Rectangle(5, -6, 15, 11) },
                new List<Rectangle> { new Rectangle(2, -7, 14, 11) },
            });
            sprite.AddFramesToAttack("punch", 3, 4);

            sprite.CreateAnimation("electricalDischarge", 0.1f, false);
            sprite.AddFrames("electricalDischarge", new List<Rectangle>
            {
                new Rectangle(335, 62, 67, 62),
                new Rectangle(402, 62, 67, 62),
                new Rectangle(0, 124, 67, 62),
                new Rectangle(67, 124, 67, 62),
                new Rectangle(134, 124, 67, 62),
                new Rectangle(201, 124, 67, 62),
                new Rectangle(268, 124, 67, 62),
            }, new[] { 0, 0, 0, 0, 0, 0, 0 }, new[] { -4, -4, -4, -4, -4, -4, -4 });
            sprite.AddAttackCollider("electricalDischarge", new List<List<Rectangle>>
            {
                new List<Rectangle> { new Rectangle(-7, -7, 18, 26) },
                new List<Rectangle> { new Rectangle(-7, -6, 20, 24) },
                new List<Rectangle> { new Rectangle(-14, -13, 35, 32) },
                new List<Rectangle> { new Rectangle(-13, -13, 34, 31) },
                new List<Rectangle> { new Rectangle(-30, -35, 64, 62) },
                new List<Rectangle> { new Rectangle(-34, -30, 69, 59) },
                new List<Rectangle> { new Rectangle(-28, -28, 59, 51) },
            });
            sprite.AddFramesToAttack("electricalDischarge", 4);

            sprite.CreateAnimation("hit", 0.1f, false);
            sprite.AddFrames("hit", new List<Rectangle>
            {
                new Rectangle(268, 186, 67, 62),
                new Rectangle(335, 186, 67, 62),
            }, new[] { 0, 0, 0, 0, 0, 0 }, new[] { -4, -4, -4, -4, -4, -4 });

            sprite.CreateAnimation("dying", 0.1f, false);
            sprite.AddFrames("dying", new List<Rectangle>
            {
                new Rectangle(402, 186, 67, 62),
                new Rectangle(0, 248, 67, 62),
                new Rectangle(67, 248, 67, 62),
                new Rectangle(134, 248, 67, 62),
                new Rectangle(201, 248, 67, 62),
                new Rectangle(201, 248, 67, 62),
                new Rectangle(268, 248, 67, 62),
            }, new[] { 0, 0, 0, 0, 0, 0, 0 }, new[] { -4, -4, -4, -4, -4, -4, -4 });

            // FSM
            _fsm = new FiniteStateMachine<EnemyOneState, EnemyOneComponent>(this, new EnemyOnePatrolState());

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
            AudioManager.hit.play(0.8f, 0.0f);
            if (dangerousStage <= 1)
            {
                (entity.scene as SceneMap)?.startScreenShake(0.7f, 100);
                base.onHit(knockback);
                FSM.changeState(new EnemyOneHitState());
            }
        }

        public override void onDeath()
        {
            FSM.changeState(new EnemyOneDyingState());
        }

        public override void update()
        {
            _fsm.update();
            base.update();
        }
    }
}
