using Nez;

namespace LudumDare40.NPCs
{
    class NpcTest : NpcBase
    {
        public NpcTest(string name) : base(name)
        {
        }

        protected override void createActionList()
        {
            message("Hello!");
            closeMessage();
        }

        protected override void loadTexture()
        {
            TextureName = Content.Characters.elliot;
        }
    }
}
