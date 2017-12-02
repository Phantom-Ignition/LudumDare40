using Nez;

namespace LudumDare40.NPCs.First_Act
{
    class ElderMouse : NpcBase
    {
        public ElderMouse(string name) : base(name)
        { }

        protected override void createActionList()
        {
            if (getGlobalSwitch("elder_requested"))
            {
                message("I will stay here, retrieve my bag, please!");
                closeMessage();
                return;
            }
            message("Hey, you! Can you please help me?");
            closeMessage();
            playerMessage("Sure, what's wrong?");
            closePlayerMessage();
            message("I lost my bag near the river. Can you find it for me?");
            message("I'm too weak, so I can't walk in the river.");
            closeMessage();
            playerMessage("I'm sorry, but I must hurry or I'll miss the guard registration.");
            closePlayerMessage();
            message("Do you want to become a real guard?");
            message("Real guards always help weaker mice, think about it.");
            closeMessage();
            playerMessage("You're right! Wait here, I'll retrieve your bag.");
            playerMessage("Err... What does it look like?");
            closePlayerMessage();
            message("It's tiny and red. It has important stuff inside, so please be careful.");
            closeMessage();
            playerMessage("Alright, stay here!");
            closePlayerMessage();
            setGlobalSwitch("elder_requested", true);
        }

        protected override void loadTexture()
        {
            TextureName = Content.Characters.elderMouse;
        }
    }
}
