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
        private bool _walljumpForcedMovement;

        //--------------------------------------------------
        // Platformer Object

        PlatformerObject _platformerObject;
        public PlatformerObject platformerObject => _platformerObject;

        //--------------------------------------------------
        // Battle Component

        private BattleComponent _battleComponent;

        //----------------------//------------------------//

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

        public void onHit()
        {
        }

        public virtual void update()
        {
            var velocity = _forceMovement ? _forceMovementVelocity.X : 0.0f;
            if (canMove() && (velocity > 0 || velocity < 0))
            {
                var po = _platformerObject;
                var mms = po.maxMoveSpeed;
                var moveSpeed = _walljumpForcedMovement ? po.gravity * mms : po.moveSpeed;

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

        private bool canMove()
        {
            return !_battleComponent.Dying;
        }
    }
}
