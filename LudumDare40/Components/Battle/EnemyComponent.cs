using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LudumDare40.Components.Player;
using LudumDare40.Components.Sprites;
using LudumDare40.Managers;
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
        private float _knockbackTick;

        //--------------------------------------------------
        // Platformer Object

        PlatformerObject _platformerObject;
        public PlatformerObject platformerObject => _platformerObject;

        //--------------------------------------------------
        // Battle Component

        private BattleComponent _battleComponent;

        //--------------------------------------------------
        // Area of Sight

        public AreaOfSightCollider areaOfSight;

        //--------------------------------------------------
        // Dangerous Stage

        public int dangerousStage;

        //----------------------//------------------------//

        public override void initialize()
        {
            dangerousStage = 1;
        }

        public override void onAddedToEntity()
        {
            _platformerObject = entity.getComponent<PlatformerObject>();

            _battleComponent = entity.getComponent<BattleComponent>();
            _battleComponent.setHp(5);
            _battleComponent.battleEntity = this;
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
            //platformerObject.velocity.X = knockback.X * 200;
            _knockbackTick = 0.1f;
            _knockbackVelocity = knockback.X * Vector2.UnitX * 60;
        }

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
            if (_knockbackTick > 0)
            {
                _knockbackTick -= Time.deltaTime;
            }

            var mms = _platformerObject.maxMoveSpeed;
            var velx = _platformerObject.velocity.X;
            if (_knockbackTick > 0 && (_knockbackVelocity.X > 0 || _knockbackVelocity.X < 0))
            {
                _platformerObject.velocity.X = MathHelper.Clamp(velx + _platformerObject.moveSpeed * _knockbackVelocity.X * Time.deltaTime, -mms, mms);
                return true;
            }

            return false;
        }

        public bool canSeeThePlayer()
        {
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
