using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace LudumDare40
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Core
    {
        public Game1() : base()
        {
        }
        
        protected override void Initialize()
        {
            base.Initialize();

            var scene = Scene.createWithDefaultRenderer(Color.CadetBlue);

            Core.scene = scene;
        }
    }
}
