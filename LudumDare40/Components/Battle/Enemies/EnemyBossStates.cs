using LudumDare40.Components.Sprites;
using LudumDare40.Extensions;
using LudumDare40.FSM;
using LudumDare40.Managers;
using LudumDare40.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System.Collections.Generic;
using Random = Nez.Random;

namespace LudumDare40.Components.Battle.Enemies
{
    public class EnemyBossStates : State<EnemyBossStates, EnemyBossComponent>
    {
        protected BoxCollider playerCollider => entity.playerCollider;

        public override void begin() { }

        public override void end() { }

        public override void update() { }
    }

    public class EnemyBossInactive : EnemyBossStates
    {
        public override void begin()
        {
            entity.sprite.play("inactive");
        }
    }

    public class EnemyBossWakingUp : EnemyBossStates
    {
        private bool _playedEffects;

        public override void begin()
        {
            Core.getGlobalManager<InputManager>().IsLocked = true;
            entity.sprite.play("wakingUp");
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                if (!_playedEffects)
                {
                    _playedEffects = true;
                    AudioManager.robot.Play(1.0f);
                    Core.startCoroutine(
                        Core.getGlobalManager<SystemManager>().flashPostProcessor.animate(1.5f)
                    );
                    Core.schedule(0.2f, t =>
                    {
                        (entity.entity.scene as SceneMap)?.blockPassage();
                    });
                }
                fsm.resetStackTo(new EnemyBossWaiting());
            }
        }

        public override void end()
        {
            entity.isBattleActive = true;
            Core.getGlobalManager<InputManager>().IsLocked = false;
        }
    }

    public class EnemyBossWaiting : EnemyBossStates
    {
        private float _attackCooldown;
        private bool _setFirstCooldown;

        public override void begin()
        {
            entity.sprite.play("stand");
        }

        public override void update()
        {
            if (!entity.canStartTheAttacks) return;
            if (!_setFirstCooldown && entity.canSeeThePlayer())
            {
                _setFirstCooldown = true;
                _attackCooldown = 1.0f;
            }
            else if (!_setFirstCooldown)
            {
                return;
            }

            if (_attackCooldown > 0.0f)
            {
                _attackCooldown -= Time.deltaTime;
                return;
            }

            float rand;

            if (!entity.canSeeThePlayer())
            {
                if (entity.hpRate() > 0.5f)
                {
                    fsm.pushState(new EnemyBossMissilesAttack());
                    _attackCooldown = 1.5f;
                }
                else
                {
                    rand = Random.nextFloat();
                    if (rand > 0.3f)
                    {
                        fsm.pushState(new EnemyBossMissilesAttack());
                        _attackCooldown = 1.5f;
                    }
                    else
                    {
                        fsm.pushState(new EnemyBossBigLaserAttack());
                        _attackCooldown = 3f;
                    }
                }
                return;
            }

            rand = Random.nextFloat();
            if (rand > 0.6f)
            {
                fsm.pushState(new EnemyBossClawAttack());
                _attackCooldown = 1f;
            }
            else if (rand > 0.4f)
            {
                fsm.pushState(new EnemyBossLaserAttack());
                _attackCooldown = 1f;
            }
            else if (entity.hpRate() > 0.5f || rand > 0.2f)
            {
                fsm.pushState(new EnemyBossMissilesAttack());
                _attackCooldown = 1.5f;
            }
            else
            {
                fsm.pushState(new EnemyBossBigLaserAttack());
                _attackCooldown = 3f;
            }
        }
    }

    public class EnemyBossClawAttack : EnemyBossStates
    {
        private bool _playedPunchSe;

        public override void begin()
        {
            entity.sprite.play("clawAttack");
        }

        public override void update()
        {
            if (!_playedPunchSe && entity.sprite.CurrentFrame == 7)
            {
                _playedPunchSe = true;
                AudioManager.punch.Play(1.0f);
                (entity.entity.scene as SceneMap)?.startScreenShake(20, 100);
            }
            if (entity.sprite.Looped)
            {
                fsm.popState();
            }
        }
    }

    public class EnemyBossLaserAttack : EnemyBossStates
    {
        private BattleComponent _playerBattler;

        public override void begin()
        {
            _playerBattler = entity.playerCollider.entity.getComponent<BattleComponent>();
            entity.sprite.play("laserAttack");
            AudioManager.laser.Play(0.9f);
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                fsm.popState();
            }

            if (_playerBattler.isOnImmunity() ||
                entity.sprite.CurrentFrame < entity.laserStartFrame ||
                entity.sprite.CurrentFrame >= entity.laserEndFrame) return;

            var index = entity.sprite.CurrentFrame - entity.laserStartFrame - 1;
            if (index < 0) return;
            var start = entity.laserPoints[index, 0] + entity.entity.position;
            var end = entity.laserPoints[index, 1] + entity.entity.position;

            RaycastHit[] hits = new RaycastHit[10];
            var hitCount = Physics.linecastAll(start, end, hits);
            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].collider.entity.tag == SceneMap.PLAYER)
                {
                    var player = hits[i].collider.entity.getComponent<BattleComponent>();
                    var knockback = new Vector2(-hits[i].normal.X, hits[i].normal.Y);
                    player.onHit(knockback);
                }
            }
        }
    }

    public class EnemyBossMissilesAttack : EnemyBossStates
    {
        private bool[] _shot;

        public override void begin()
        {
            _shot = new[] {false, false, false};
            entity.sprite.play("missilesLaunch");
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                fsm.popState();
            }
            
            if (!_shot[0] && entity.sprite.CurrentFrame == 3)
            {
                _shot[0] = true;
                entity.launchMissiles(0);
                AudioManager.missile.Play(0.9f, 0.0f, 0.0f);
            }
            if (!_shot[1] && entity.sprite.CurrentFrame == 5)
            {
                _shot[1] = true;
                entity.launchMissiles(1);
                AudioManager.missile.Play(0.9f, 0.3f, 0.0f);
            }
            if (!_shot[2] && entity.sprite.CurrentFrame == 7)
            {
                _shot[2] = true;
                entity.launchMissiles(2);
                AudioManager.missile.Play(0.9f, 0.5f, 0.0f);
            }
        }
    }

    public class EnemyBossBigLaserAttack : EnemyBossStates
    {
        private BattleComponent _playerBattler;
        private AnimatedSprite _bigLaserSprite;
        private bool _playedLaserSe;

        public override void begin()
        {
            createLaserSprite();
            _playerBattler = entity.playerCollider.entity.getComponent<BattleComponent>();
            entity.sprite.play("bigLaserAttack");
        }

        private void createLaserSprite()
        {
            var texture = entity.entity.scene.content.Load<Texture2D>(Content.Misc.bigLaser);
            var laser = entity.entity.scene.createEntity("big-laser-sprite");

            _bigLaserSprite = laser.addComponent(new AnimatedSprite(texture, "default"));
            _bigLaserSprite.CreateAnimation("default", 0.08f, false);
            _bigLaserSprite.AddFrames("default", new List<Rectangle>()
            {
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 101, 8),
                new Rectangle(0, 0, 101, 8),
                new Rectangle(0, 0, 101, 8),
                new Rectangle(0, 0, 101, 8),
                new Rectangle(0, 0, 101, 8),
                new Rectangle(0, 0, 101, 8),
                new Rectangle(0, 0, 101, 8),
                new Rectangle(0, 8, 101, 8),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
                new Rectangle(0, 0, 0, 0),
            });
            laser.setScale(new Vector2(2.5f, 1));
            laser.position = entity.entity.position + new Vector2(-143, 24);
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                fsm.popState();
            }
            
            if (!_playedLaserSe && entity.sprite.CurrentFrame >= 20)
            {
                _playedLaserSe = true;
                AudioManager.laser.Play(0.9f);
            }

            if (_playerBattler.isOnImmunity() || entity.sprite.CurrentFrame < 23 || entity.sprite.CurrentFrame > 30) return;
            
            var start = new Vector2(entity.entity.position.X - 600, entity.entity.position.Y + 21);
            var end = new Vector2(entity.entity.position.X + 10, entity.entity.position.Y + 20);

            RaycastHit[] hits = new RaycastHit[10];
            var hitCount = Physics.linecastAll(start, end, hits);
            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].collider.entity.tag == SceneMap.PLAYER)
                {
                    var player = hits[i].collider.entity.getComponent<BattleComponent>();
                    var knockback = new Vector2(-hits[i].normal.X, hits[i].normal.Y);
                    player.onHit(knockback);
                }
            }
        }

        public override void end()
        {
            _bigLaserSprite.entity.setEnabled(false);
        }
    }

    public class EnemyBossDying : EnemyBossStates
    {
        public override void begin()
        {
            entity.sprite.play("dying");
            Core.getGlobalManager<InputManager>().IsLocked = true;
        }

        public override void end()
        {
            Core.getGlobalManager<InputManager>().IsLocked = false;
        }
    }
}
