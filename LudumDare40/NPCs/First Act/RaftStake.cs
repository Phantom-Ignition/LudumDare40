using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;

namespace LudumDare40.NPCs.First_Act
{
    class RaftStake : NpcBase
    {
        public RaftStake(string name) : base(name)
        { }

        protected override void createAnimations()
        {
            sprite.CreateAnimation("stand");
            sprite.AddFrames("stand", new List<Rectangle>()
            {
                new Rectangle(0, 0, 29, 35)
            }, new int[] { 0 }, new int[] { 8 });
        }

        protected override void createActionList()
        {
            playerMessage("This rope doesn't seem strong enough for a raft.");
            closePlayerMessage();
        }

        protected override void loadTexture()
        {
            //TextureName = Content.Misc.stakeWithRope;
        }
    }
}
