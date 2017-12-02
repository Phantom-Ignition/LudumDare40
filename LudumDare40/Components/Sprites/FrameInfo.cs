using System.Collections.Generic;
using LudumDare40.Components.Battle;
using Nez.Textures;

namespace LudumDare40.Components.Sprites
{
    public class FrameInfo
    {
        public Subtexture Subtexture { get; }
        public List<AttackCollider> AttackColliders { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        public FrameInfo(Subtexture subtexture, int offsetX, int offsetY)
        {
            AttackColliders = new List<AttackCollider>();
            Subtexture = subtexture;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }
}
