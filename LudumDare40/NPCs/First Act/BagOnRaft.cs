using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;

namespace LudumDare40.NPCs.First_Act
{
    class BagOnRaft : NpcBase
    {
        public BagOnRaft(string name) : base(name)
        { }

        protected override void createActionList()
        {
            cinematicIn(10.0f, 1.0f);
            playerMessage("Oh, I found the bag!");
            closePlayerMessage();
            wait(0.5f);
            hideTexture();
            setGlobalSwitch("picked_up_bag", true);
            playerMessage("Wait, why is the raft moving?!");
            playerMessage("I don't know how to swim!");
            movePlayer(-10.0f * Vector2.UnitX);
            wait(0.3f);
            movePlayer(Vector2.Zero);
            setGlobalSwitch("replace_raft_right_barrier", true);
            playerMessage("Where am I going?!");
            closePlayerMessage();
            setGlobalSwitch("can_rain", true);
            wait(4.0f);
            playerMessage("Why did it have to rain right now?!");
            closePlayerMessage();
            setGlobalSwitch("can_rain_higher", true);
            wait(2.0f);
            // Set new graphic
            playerMessage("The rain is getting worst!");
            closePlayerMessage();
            wait(1.0f);
            playerMessage("Damn it! Is it my end?");
            closePlayerMessage();
            wait(1.0f);
            //mapTransfer()
        }

        protected override void createAnimations()
        {
            sprite.CreateAnimation("stand");
            sprite.AddFrames("stand", new List<Rectangle>()
            {
                new Rectangle(0, 0, 12, 11)
            }, new int[] { 0 }, new int[] { 0 });
        }

        protected override void createCollider()
        {
            entity.addComponent(new BoxCollider(-ObjectRect.Width / 2, -ObjectRect.Height / 2, ObjectRect.Width, ObjectRect.Height));
        }

        protected override void loadTexture()
        {
           // TextureName = Content.Misc.bag;
        }
    }
}
