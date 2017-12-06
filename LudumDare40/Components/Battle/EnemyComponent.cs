using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LudumDare40.Components.Player;
using LudumDare40.Components.Sprites;
using LudumDare40.Managers;
using LudumDare40.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace LudumDare40.Components.Battle
{
    public class EnemyComponent : Component, IBattleEntity, IUpdatable
    {
        //--------------------------------------------------
        // Sprite

        public AnimatedSprite sprite;

        //--------------------------------------------------
        // Player collider reference

        public BoxCollider playerCollider;

        //--------------------------------------------------
        // Forced Movement

        private bool _forceMovement;
        private Vector2 _forceMovementVelocity;

        //--------------------------------------------------
        // Knockback

        private Vector2 _knockbackVelocity;
        private Vector2 _knockbackTick;

        //--------------------------------------------------
        // Platformer Object

        PlatformerObject _platformerObject;
        public PlatformerObject platformerObject => _platformerObject;

        //--------------------------------------------------
        // Battle Component

        protected BattleComponent _battleComponent;

        //--------------------------------------------------
        // Area of Sight

        public AreaOfSightCollider areaOfSight;

        //--------------------------------------------------
        // Patrol

        public float patrolTime;
        public bool patrolStartRight;

        //--------------------------------------------------
        // Dangerous Stage

        public int dangerousStage;

        //--------------------------------------------------
        // Can take damage

        public virtual bool canTakeDamage => true;

        //----------------------//------------------------//

        public EnemyComponent(bool patrolStartRight)
        {
            this.patrolStartRight = patrolStartRight;
        }

        public override void initialize()
        {
            dangerousStage = 1;
        }

        public override void onAddedToEntity()
        {
            _platformerObject = entity.getComponent<PlatformerObject>();

            _battleComponent = entity.getComponent<BattleComponent>();
            _battleComponent.setHp(4);
            _battleComponent.battleEntity = this;

            entity.setTag(SceneMap.ENEMIES);
        }

        public void increaseDangerousStage()
        {
            dangerousStage++;
        }

        public void forceMovement(Vector2 velocity)
        {
            if (velocity == Vector2.Zero)
            {
                _forceMovement = false;
            }
            else
            {
                _forceMovement = true;
                _forceMovementVelocity = velocity;
            }
        }

        public virtual void onHit(Vector2 knockback)
        {
            _knockbackTick = new Vector2(0.06f, 0.1f);
            _knockbackVelocity = new Vector2(knockback.X * 60, -5);
        }

        public virtual void onDeath() { }

        public virtual void update()
        {
            if (areaOfSight != null)
            {
                var offsetX = 0.0f;
                if (sprite.spriteEffects == SpriteEffects.FlipHorizontally)
                    offsetX = -2.0f * areaOfSight.X;
                areaOfSight.ApplyOffset(offsetX, 0);
            }

            // apply knockback before movement
            if (applyKnockback())
                return;

            var velocity = _forceMovement ? _forceMovementVelocity.X : 0.0f;
            if (canMove() && (velocity > 0 || velocity < 0))
            {
                var po = _platformerObject;
                var mms = po.maxMoveSpeed;
                var moveSpeed = po.moveSpeed;

                if (velocity != Math.Sign(po.velocity.X))
                {
                    po.velocity.X = 0;
                }

                po.velocity.X = (int)MathHelper.Clamp(po.velocity.X + moveSpeed * velocity * Time.deltaTime, -mms, mms);
                sprite.spriteEffects = velocity < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }
            else
            {
                _platformerObject.velocity.X = 0;
            }
        }

        private bool applyKnockback()
        {
            if (_knockbackTick.X > 0)
            {
                _knockbackTick.X -= Time.deltaTime;
            }
            if (_knockbackTick.Y > 0)
            {
                _knockbackTick.Y -= Time.deltaTime;
            }

            var mms = _platformerObject.maxMoveSpeed;
            var velx = _platformerObject.velocity.X;
            var vely = _platformerObject.velocity.Y;
            bool appliedKb = false;
            if (_knockbackTick.X > 0)
            {
                _platformerObject.velocity.X = MathHelper.Clamp(velx + 1000 * _knockbackVelocity.X * Time.deltaTime, -mms, mms);
                appliedKb = true;
            }
            if (_knockbackTick.Y > 0)
            {
                _platformerObject.velocity.Y = MathHelper.Clamp(vely + 1000 * _knockbackVelocity.Y * Time.deltaTime, -mms, mms);
                appliedKb = true;
            }
            return appliedKb;
        }

        public bool canSeeThePlayer()
        {
            if (!playerCollider.entity.enabled) return false;
            CollisionResult collisionResult;
            return areaOfSight.collidesWith(playerCollider, out collisionResult);
        }

        public float distanceToPlayer()
        {
            return playerCollider.entity.position.X - entity.position.X;
        }

        private bool canMove()
        {
            return !_battleComponent.Dying;
        }
    }
}
