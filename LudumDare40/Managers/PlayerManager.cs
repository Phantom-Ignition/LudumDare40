using Nez;

namespace LudumDare40.Managers
{
    class PlayerManager : IUpdatableManager
    {
        private bool _holdingCore;
        public bool HoldingCore
        {
            get => _holdingCore;
            set
            {
                _holdingCore = value;
                if (value)
                {
                    AudioManager.equip.Play();
                }
            }
        }

        public void update() { }
    }
}
