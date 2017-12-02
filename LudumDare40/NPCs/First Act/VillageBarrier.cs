using Microsoft.Xna.Framework;

namespace LudumDare40.NPCs.First_Act
{
    class VillageBarrier : NpcBase
    {
        public VillageBarrier(string name) : base(name)
        {
            RunOnTouch = true;
            Invisible = true;
        }

        protected override void createActionList()
        {
            movePlayer(-Vector2.UnitX);
            wait(0.3f);
            movePlayer(Vector2.Zero);
            if (getGlobalSwitch("elder_requested"))
            {
                playerMessage("I must find the bag!");
            }
            else
            {
                playerMessage("I should talk with her, she seems worried.");
            }
            closePlayerMessage();
        }

        protected override void loadTexture()
        { }
    }
}
