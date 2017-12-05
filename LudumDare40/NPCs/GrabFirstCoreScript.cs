namespace LudumDare40.NPCs
{
    class GrabFirstCoreScript : NpcBase
    {
        public GrabFirstCoreScript(string name) : base(name)
        {
            Invisible = true;
        }

        protected override void loadTexture() { }

        protected override void createActionList()
        {
            playerMessage("These cores are too heavy, I can't jump on walls with them.");
            closePlayerMessage();
        }
    }
}
