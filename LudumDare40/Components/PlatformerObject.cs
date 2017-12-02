using LudumDare40.Components.Map;
using LudumDare40.Managers;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace LudumDare40.Components
{
    public class PlatformerObject : Component, IUpdatable
    {
        //--------------------------------------------------
        // Physics

        public float moveSpeed = 1000;
        public float maxMoveSpeed = 150;
        public float gravity = 1200;
        public float jumpHeight = 16 * 3;
        public float wallGravity = 100;

        //--------------------------------------------------
        // Walljump

        public bool grabbingWall;

        //--------------------------------------------------
        // Ladder
        
        public LadderComponent ladderComponent { get; set; }
        public bool IsLadderTouching => ladderComponent != null;

        public bool gabbingLadder;

        //--------------------------------------------------
        // Tiled Mover

        TiledMapMover _mover;
        public TiledMapMover mover => _mover;

        //--------------------------------------------------
        // Box Collider

        BoxCollider _boxCollider;
        
        //--------------------------------------------------
        // Velocity

        public Vector2 velocity;
        
        //--------------------------------------------------
        // Collision State

        public TiledMapMover.CollisionState collisionState = new TiledMapMover.CollisionState();

        //----------------------//------------------------//

        public override void onAddedToEntity()
        {
            _mover = this.getComponent<TiledMapMover>();
            _boxCollider = entity.getComponent<BoxCollider>();
        }

        public void update()
        {
            if (gabbingLadder)
            {
                // deny any movement in the x axis
                velocity.X = 0.0f;
            }
            else
            {
                // apply gravity
                velocity.Y += ((grabbingWall && velocity.Y > 0) ? wallGravity : gravity) * Time.deltaTime;
            }

            // apply movement
            _mover.move(velocity * Time.deltaTime, _boxCollider, collisionState);

            // handle map bounds
            var map = Core.getGlobalManager<SystemManager>().TiledMap;
            var x = MathHelper.Clamp(_mover.transform.position.X, 0, map.widthInPixels);
            _mover.transform.position = new Vector2(x, _mover.transform.position.Y);

            // update velocity
            if (collisionState.right || collisionState.left)
                velocity.X = 0;
            if (collisionState.above || collisionState.below)
                velocity.Y = 0;
        }

        public void jump()
        {
            velocity.Y = -Mathf.sqrt(2 * jumpHeight * gravity);
        }

        public void enterOnLadder()
        {
            gabbingLadder = true;
            velocity.Y = 0;
            _mover.move(Vector2.UnitY * -1, _boxCollider, collisionState);

            var ladderX = ladderComponent.transform.position.X + ladderComponent.size.X / 2;
            var ladderDelta = ladderX -_mover.transform.position.X;
            _mover.move(Vector2.UnitX * ladderDelta, _boxCollider, collisionState);
        }

        public void ladderVelocityUp()
        {
            _mover.move(Vector2.UnitY * -2, _boxCollider, collisionState);
        }

        public void ladderVelocityDown()
        {
            _mover.move(Vector2.UnitY * 2, _boxCollider, collisionState);
        }
    }
}
