using LudumDare40.Components.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System.Collections.Generic;
using LudumDare40.FSM;
using LudumDare40.Managers;
using LudumDare40.Scenes;
using LudumDare40.Extensions;

namespace LudumDare40.Components.Battle.Enemies
{
    public class EnemyDroneComponent : EnemyComponent
    {
        //--------------------------------------------------
        // Finite State Machine

        private FiniteStateMachine<EnemyDroneState, EnemyDroneComponent> _fsm;
        public FiniteStateMachine<EnemyDroneState, EnemyDroneComponent> FSM => _fsm;

        //----------------------//------------------------//

        public EnemyDroneComponent(bool patrolStartRight) : base(patrolStartRight)
        {
        }

        public override void initialize()
        {
            base.initialize();

            // sprite init
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.enemyDrone);
            sprite = entity.addComponent(new AnimatedSprite(texture, "floating"));
            sprite.CreateAnimation("floating", 0.2f);
            sprite.AddFrames("floating", new List<Rectangle>
            {
                new Rectangle(0, 0, 64, 64),
                new Rectangle(64, 0, 64, 64),
                new Rectangle(128, 0, 64, 64),
                new Rectangle(192, 0, 64, 64),
                new Rectangle(256, 0, 64, 64),
                new Rectangle(320, 0, 64, 64),
            }, new[] { 0, 0, 0, 0, 0, 0 }, new[] { -28, -28, -28, -28, -28, -28 });

            sprite.CreateAnimation("attack", 0.1f, false);
            sprite.AddFrames("attack", new List<Rectangle>
            {
                new Rectangle(0, 64, 64, 64),
                new Rectangle(64, 64, 64, 64),
                new Rectangle(128, 64, 64, 64),
                new Rectangle(192, 64, 64, 64),
                new Rectangle(256, 64, 64, 64),
                new Rectangle(320, 64, 64, 64),
            }, new[] { 0, 0, 0, 0, 0, 0 }, new[] { -28, -28, -28, -28, -28, -28 });

            sprite.CreateAnimation("attackStrong", 0.1f, false);
            sprite.AddFrames("attackStrong", new List<Rectangle>
            {
                new Rectangle(256, 128, 64, 64),
                new Rectangle(320, 128, 64, 64),
                new Rectangle(0, 192, 64, 64),
                new Rectangle(64, 192, 64, 64),
                new Rectangle(128, 192, 64, 64),
                new Rectangle(192, 192, 64, 64),
                new Rectangle(256, 192, 64, 64),
                new Rectangle(320, 192, 64, 64),
            }, new[] { 0, 0, 0, 0, 0, 0, 0, 0 }, new[] { -28, -28, -28, -28, -28, -28, -28, -28 });

            sprite.CreateAnimation("hit", 0.1f, false);
            sprite.AddFrames("hit", new List<Rectangle>
            {
                new Rectangle(0, 128, 64, 64),
                new Rectangle(64, 128, 64, 64),
                new Rectangle(128, 128, 64, 64),
                new Rectangle(192, 128, 64, 64),
            }, new[] { 0, 0, 0, 0 }, new[] { -28, -28, -28, -28 });

            sprite.CreateAnimation("dying", 0.1f, false);
            sprite.AddFrames("dying", new List<Rectangle>
            {
                new Rectangle(0, 256, 64, 64),
                new Rectangle(64, 256, 64, 64),
                new Rectangle(128, 256, 64, 64),
                new Rectangle(192, 256, 64, 64),
                new Rectangle(256, 256, 64, 64),
                new Rectangle(320, 256, 64, 64),
                new Rectangle(0, 320, 64, 64),
            }, new[] { 0, 0, 0, 0, 0, 0, 0 }, new[] { -28, -28, -28, -28, -28, -28, -28 });

            // collisor init
            var collider = entity.addComponent(new HurtCollider(-17f, -39f, 33f, 23f));
            Flags.setFlagExclusive(ref collider.collidesWithLayers, 2);
            Flags.setFlagExclusive(ref collider.physicsLayer, 2);

            // FSM
            _fsm = new FiniteStateMachine<EnemyDroneState, EnemyDroneComponent>(this, new EnemyDronePatrolState());

            // View range
            areaOfSight = entity.addComponent(new AreaOfSightCollider(-24, -28, 110, 48));
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            // Change move speed
            platformerObject.maxMoveSpeed = 60;
            platformerObject.moveSpeed = 60;
        }

        public void createShot(int type)
        {
            if (type == 1)
            {
                AudioManager.shot.Play(0.7f);
            }
            else
            {
                AudioManager.cannon.Play(0.7f);
            }
            var shots = entity.scene.findEntitiesWithTag(SceneMap.SHOTS);
            var shot = entity.scene.createEntity($"shot:${shots.Count}");
            var direction = sprite.spriteEffects == SpriteEffects.FlipHorizontally ? -1 : 1;
            shot.addComponent(new ShotComponent(direction, type));
            sprite.play(type == 1 ? "attack" : "attackStrong");

            var position = entity.getComponent<HurtCollider>().absolutePosition;
            shot.transform.position = position;
            if (sprite.spriteEffects == SpriteEffects.FlipHorizontally)
            {
                shot.transform.position += new Vector2(-12, 12);
            }
            else
            {
                shot.transform.position += 12 * Vector2.One;
            }
        }

        public override void onHit(Vector2 knockback)
        {
            base.onHit(knockback);
            FSM.changeState(new EnemyDroneHitState());
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
