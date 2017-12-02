using Microsoft.Xna.Framework;
using Nez;
using Nez.PhysicsShapes;

namespace LudumDare40.Components.Battle
{
    public class AttackCollider : BoxCollider
    {
        public Vector2 _originalLocalOffset;

        public int X => (int) _originalLocalOffset.X;
        public int Y => (int)_originalLocalOffset.Y;

        public AttackCollider(float x, float y, float width, float height) : base(x, y, width, height)
        {
            _originalLocalOffset = new Vector2(_localOffset.X, _localOffset.Y);
        }

        public void ApplyOffset(float x, float y)
        {
            setLocalOffset(_originalLocalOffset + new Vector2(x, y));
        }

        public override void debugRender(Graphics graphics) { }

        public void debugRender(Graphics graphics, bool draw)
        {
            var poly = shape as Polygon;
            graphics.batcher.drawHollowRect(bounds, Debug.Colors.colliderBounds, Debug.Size.lineSizeMultiplier);
            graphics.batcher.drawRect(new Rectangle((int)bounds.x-1, (int)bounds.y-1, (int)bounds.width+1, (int)bounds.height+1), Color.DarkOrchid * 0.5f);
            graphics.batcher.drawString(graphics.bitmapFont, _localOffset.ToString(), absolutePosition, Color.White);
            //if (draw) base.debugRender(graphics);
        }
    }
}
