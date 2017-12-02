using LudumDare40.Managers;
using Nez;

namespace LudumDare40.NPCs.First_Act
{
    class ElliotMother : NpcBase
    {
        public ElliotMother(string name) : base(name)
        {
        }

        protected override void createActionList()
        {
            if (getSwitch("started2"))
            {
                message("Hey, you will be late!");
                closeMessage();
            }
            else if (getSwitch("started"))
            {
                message("Honestly, I don't think this is your thing, maybe you are dreaming too high.");
                message("What's the problem with be an ordinary mouse?");
                setSwitch("started2", true);
                closeMessage();
            }
            else
            {
                cinematicIn(30, 1);
                wait(2);
                focusCamera(entity);
                message("Elliot, you will be late to the guard register, stop poking this thing and hurry!");
                closeMessage();
                cinematicOut(0, 1);
                focusCamera(Core.getGlobalManager<SystemManager>().playerEntity);
                setSwitch("started", true);
            }
        }

        protected override void loadTexture()
        {
            TextureName = Content.Characters.elliotMother;
        }
    }
}
