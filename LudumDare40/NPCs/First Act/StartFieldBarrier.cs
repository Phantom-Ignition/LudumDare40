using Microsoft.Xna.Framework;

namespace LudumDare40.NPCs.First_Act
{
    class StartFieldBarrier : NpcBase
    {
        public StartFieldBarrier(string name) : base(name)
        {
            RunOnTouch = true;
            Invisible = true;
        }

        protected override void createActionList()
        {
            movePlayer(Vector2.UnitX);
            wait(0.3f);
            movePlayer(Vector2.Zero);
            if (getGlobalSwitch("elder_requested"))
            {
                playerMessage("I must find the bag!");
            }
            else
            {
                playerMessage("I must hurry!");
            }
            closePlayerMessage();
        }

        protected override void loadTexture()
        { }
    }
}
