using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace LudumDare40.Components.Map
{
    public class LadderComponent : Component
    {
        private TiledObject _tiledObject;
        public Vector2 size => new Vector2(_tiledObject.width, _tiledObject.height);

        public LadderComponent(TiledObject tiledObject)
        {
            _tiledObject = tiledObject;
        }

        public override void onAddedToEntity()
        {
            entity.addComponent(new BoxCollider(0, 0, _tiledObject.width, _tiledObject.height));
            transform.position = _tiledObject.position;
        }
    }
}
