namespace LudumDare40.NPCs
{
    class StartGameScript : NpcBase
    {
        public StartGameScript(string name) : base(name)
        {
            Invisible = true;
        }

        protected override void loadTexture() { }

        protected override void createActionList()
        {
            cinematicIn(30, 1);
            playerMessage("I must find 3 cores for these reactors, so I can open the gate and get out of here.");
            playerMessage("By the look of this old guardian, it seems to be an abandoned place, there may be have some patrol robots, so I must be careful.");
            closePlayerMessage();
            wait(0.5f);
            cinematicOut(0, 1);
        }
    }
}
