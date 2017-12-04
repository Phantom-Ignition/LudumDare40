using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LudumDare40.Components.Sprites;
using LudumDare40.FSM;
using LudumDare40.Scenes;
using Microsoft.Xna.Framework;
using Nez;
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

    public class EnemyBossWaiting : EnemyBossStates
    {
        private float _attackCooldown;

        public override void begin()
        {
            entity.sprite.play("stand");
        }

        public override void update()
        {
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
            else if (rand > 0.2f)
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
        public override void begin()
        {
            entity.sprite.play("clawAttack");
        }

        public override void update()
        {
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
            }
            if (!_shot[1] && entity.sprite.CurrentFrame == 5)
            {
                _shot[1] = true;
                entity.launchMissiles(1);
            }
            if (!_shot[2] && entity.sprite.CurrentFrame == 7)
            {
                _shot[2] = true;
                entity.launchMissiles(2);
            }
        }
    }

    public class EnemyBossBigLaserAttack : EnemyBossStates
    {
        private BattleComponent _playerBattler;

        public override void begin()
        {
            _playerBattler = entity.playerCollider.entity.getComponent<BattleComponent>();
            entity.sprite.play("bigLaserAttack");
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
            var start = new Vector2(0, entity.entity.position.Y + 20);
            var end = new Vector2(Scene.virtualSize.X, entity.entity.position.Y + 20);

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
}
