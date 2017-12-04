using LudumDare40.Components.Sprites;
using LudumDare40.FSM;
using LudumDare40.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System.Collections.Generic;

namespace LudumDare40.Components.Battle.Enemies
{
    public class EnemyBossComponent : EnemyComponent
    {
        //--------------------------------------------------
        // Finite State Machine
        
        private FiniteStateMachine<EnemyBossStates, EnemyBossComponent> _fsm;
        public FiniteStateMachine<EnemyBossStates, EnemyBossComponent> FSM => _fsm;

        //--------------------------------------------------
        // HP

        private const int _maxHp = 10;

        //--------------------------------------------------
        // Laser

        public int laserStartFrame { get; private set; }
        public int laserEndFrame { get; private set; }
        public Vector2[,] laserPoints { get; private set; }

        //--------------------------------------------------
        // Battle Active

        public bool isBattleActive;

        //----------------------//------------------------//

        public EnemyBossComponent(bool patrolStartRight) : base(patrolStartRight) { }

        public override void initialize()
        {
            base.initialize();

            // Init sprite
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.boss);
            sprite = entity.addComponent(new AnimatedSprite(texture, "stand"));
            sprite.CreateAnimation("stand", 0.25f);
            sprite.AddFrames("stand", new List<Rectangle>
            {
                new Rectangle(0, 0, 235, 140),
                new Rectangle(235, 0, 235, 140),
                new Rectangle(470, 0, 235, 140),
                new Rectangle(705, 0, 235, 140),
                new Rectangle(940, 0, 235, 140),
                new Rectangle(1175, 0, 235, 140),
            });

            sprite.CreateAnimation("clawAttack", 0.08f, false);
            sprite.AddFrames("clawAttack", new List<Rectangle>
            {
                new Rectangle(1410, 0, 235, 140),
                new Rectangle(1645, 0, 235, 140),
                new Rectangle(1880, 0, 235, 140),
                new Rectangle(2115, 0, 235, 140),
                new Rectangle(2350, 0, 235, 140),
                new Rectangle(2585, 0, 235, 140),
                new Rectangle(0, 140, 235, 140),
                new Rectangle(235, 140, 235, 140),
                new Rectangle(470, 140, 235, 140),
                new Rectangle(705, 140, 235, 140),
                new Rectangle(940, 140, 235, 140),
                new Rectangle(1175, 140, 235, 140),
            });
            sprite.AddAttackCollider("clawAttack", new List<List<Rectangle>>()
            {
                new List<Rectangle>() { new Rectangle(0, 0, 0, 0) },
                new List<Rectangle>() { new Rectangle(0, 0, 0, 0) },
                new List<Rectangle>() { new Rectangle(0, 0, 0, 0) },
                new List<Rectangle>() { new Rectangle(0, 0, 0, 0) },
                new List<Rectangle>() { new Rectangle(0, 0, 0, 0) },
                new List<Rectangle>() { new Rectangle(0, 0, 0, 0) },
                new List<Rectangle>() { new Rectangle(-62, -50, 135, 60) },
                new List<Rectangle>() { new Rectangle(-82, -24, 155, 60) },
                new List<Rectangle>() { new Rectangle(-62, -24, 135, 60) },
            });
            sprite.AddFramesToAttack("clawAttack", 6, 7, 8);

            sprite.CreateAnimation("laserAttack", 0.08f, false);
            sprite.AddFrames("laserAttack", new List<Rectangle>
            {
                new Rectangle(1410, 140, 235, 140),
                new Rectangle(1645, 140, 235, 140),
                new Rectangle(1880, 140, 235, 140),
                new Rectangle(2115, 140, 235, 140),
                new Rectangle(2350, 140, 235, 140),
                new Rectangle(2585, 140, 235, 140),
                new Rectangle(0, 280, 235, 140),
                new Rectangle(235, 280, 235, 140),
                new Rectangle(470, 280, 235, 140),
                new Rectangle(705, 280, 235, 140),
                new Rectangle(940, 280, 235, 140),
                new Rectangle(1175, 280, 235, 140),
                new Rectangle(1410, 280, 235, 140),
                new Rectangle(1645, 280, 235, 140),
                new Rectangle(1880, 280, 235, 140),
                new Rectangle(2115, 280, 235, 140),
                new Rectangle(2350, 280, 235, 140),
                new Rectangle(2585, 280, 235, 140),
                new Rectangle(0, 420, 235, 140),
            });

            sprite.CreateAnimation("bigLaserAttack", 0.08f, false);
            sprite.AddFrames("bigLaserAttack", new List<Rectangle>
            {
                new Rectangle(470, 840, 235, 140),
                new Rectangle(705, 840, 235, 140),
                new Rectangle(940, 840, 235, 140),
                new Rectangle(1175, 840, 235, 140),
                new Rectangle(1410, 840, 235, 140),
                new Rectangle(1645, 840, 235, 140),
                new Rectangle(1880, 840, 235, 140),
                new Rectangle(1410, 840, 235, 140),
                new Rectangle(1645, 840, 235, 140),
                new Rectangle(1880, 840, 235, 140),
                new Rectangle(2115, 840, 235, 140),
                new Rectangle(2350, 840, 235, 140),
                new Rectangle(2350, 840, 235, 140),
                new Rectangle(2350, 840, 235, 140),
                new Rectangle(2350, 840, 235, 140),
                new Rectangle(2350, 840, 235, 140),
                new Rectangle(2350, 840, 235, 140),
                new Rectangle(2350, 840, 235, 140),
                new Rectangle(2585, 840, 235, 140),
                new Rectangle(0, 980, 235, 140),
                new Rectangle(235, 980, 235, 140),
                new Rectangle(470, 980, 235, 140),
            });

            sprite.CreateAnimation("missilesLaunch", 0.1f);
            sprite.AddFrames("missilesLaunch", new List<Rectangle>
            {
                new Rectangle(235, 700, 235, 140),
                new Rectangle(470, 700, 235, 140),
                new Rectangle(705, 700, 235, 140),
                new Rectangle(940, 700, 235, 140),
                new Rectangle(1175, 700, 235, 140),
                new Rectangle(1410, 700, 235, 140),
                new Rectangle(1645, 700, 235, 140),
                new Rectangle(1880, 700, 235, 140),
                new Rectangle(2115, 700, 235, 140),
                new Rectangle(2350, 700, 235, 140),
                new Rectangle(2585, 700, 235, 140),
                new Rectangle(0, 840, 235, 140),
                new Rectangle(235, 840, 235, 140),
            });

            sprite.CreateAnimation("dying", 0.1f, false);
            sprite.AddFrames("dying", new List<Rectangle>
            {
                new Rectangle(235, 420, 235, 140),
                new Rectangle(470, 420, 235, 140),
                new Rectangle(705, 420, 235, 140),
                new Rectangle(940, 420, 235, 140),
                new Rectangle(1175, 420, 235, 140),
                new Rectangle(1410, 420, 235, 140),
                new Rectangle(1645, 420, 235, 140),
                new Rectangle(1880, 420, 235, 140),
                new Rectangle(2115, 420, 235, 140),
                new Rectangle(2350, 420, 235, 140),
                new Rectangle(2585, 420, 235, 140),
                new Rectangle(0, 560, 235, 140),
                new Rectangle(235, 560, 235, 140),
                new Rectangle(470, 560, 235, 140),
                new Rectangle(705, 560, 235, 140),
                new Rectangle(940, 560, 235, 140),
                new Rectangle(1175, 560, 235, 140),
                new Rectangle(1410, 560, 235, 140),
                new Rectangle(1645, 560, 235, 140),
                new Rectangle(1880, 560, 235, 140),
                new Rectangle(2115, 560, 235, 140),
            });

            // config the laser 
            laserStartFrame = 8;
            laserEndFrame = 16;
            laserPoints = new[,]
            {
                {new Vector2(16, -4), new Vector2(16, 47)},
                {new Vector2(10, -5), new Vector2(-5, 47)},
                {new Vector2(6, -2), new Vector2(-21, 47)},
                {new Vector2(0, -3), new Vector2(-42, 47)},
                {new Vector2(-3, -7), new Vector2(-52, 47)},
                {new Vector2(-6, -8), new Vector2(-85, 47)},
                {new Vector2(-10, -10), new Vector2(-117, 47)},
            };

            // FSM
            _fsm = new FiniteStateMachine<EnemyBossStates, EnemyBossComponent>(this, new EnemyBossWaiting());

            // add hurt box
            entity.addComponent(new HurtCollider(-30, -55, 146, 102));

            // View range
            areaOfSight = entity.addComponent(new AreaOfSightCollider(-120, -50, 250, 96));

            // Deaticvate from battle
            isBattleActive = false;
        }

        public override void update()
        {
            _fsm.update();
            base.update();
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            _battleComponent.setHp(_maxHp);

            var collider = entity.getComponent<BoxCollider>();
            collider.height += 27;
        }

        public void launchMissiles(int index)
        {
            var shots = entity.scene.findEntitiesWithTag(SceneMap.SHOTS);
            var shot = entity.scene.createEntity($"shot:${shots.Count}");

            var rotations = new[]
            {
                -9.0f,
                -15.0f,
                -20.0f,
            };

            var missile = shot.addComponent(new MissileComponent(rotations[index]));

            var basePosition = entity.getComponent<HurtCollider>().absolutePosition;
            var positions = new[]
            {
                new Vector2(8, -30),
                new Vector2(13, -26),
                new Vector2(18, -20),
            };

            missile.setInitialPosition(basePosition + positions[index]);
            shot.transform.position = basePosition + positions[index];
        }

        public float hpRate()
        {
            if (_battleComponent == null) return 1;
            return _battleComponent.HP / _maxHp;
        }

        public override void onHit(Vector2 knockback) { }

        public override void onDeath()
        {
            FSM.changeState(new EnemyBossDying());
        }

        public override void debugRender(Graphics graphics)
        {
            base.debugRender(graphics);

            if (sprite.CurrentFrame < laserStartFrame) return;
            if (sprite.CurrentFrame >= laserEndFrame) return;

            int index;
            Vector2 start;
            Vector2 end;

            if (sprite.CurrentAnimation == "bigLaserAttack")
            {
                if (sprite.CurrentFrame < 10 || sprite.CurrentFrame > 17) return;
                start = new Vector2(entity.position.X - 300, entity.position.Y + 21);
                end = new Vector2(entity.position.X + 10, entity.position.Y + 21);

                graphics.batcher.drawLine(start, end, Color.Black);
                return;
            }

            if (sprite.CurrentAnimation != "laserAttack") return;

            index = sprite.CurrentFrame - laserStartFrame-1;
            if (index < 0) return;
            start = laserPoints[index, 0] + entity.position;
            end = laserPoints[index, 1] + entity.position;

            graphics.batcher.drawLine(start, end, Color.Black);
        }
    }
}
