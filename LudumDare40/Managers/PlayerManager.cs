using Nez;

namespace LudumDare40.Managers
{
    class PlayerManager : IUpdatableManager
    {
        public const int MaxHp = 5;

        private int _hp;
        public int hp => _hp;

        public PlayerManager()
        {
        }

        public void update()
        {
        }
    }
}
