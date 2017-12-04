using Nez;

namespace LudumDare40.Managers
{
    public class PlayerManager : IUpdatableManager
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

        public int CoresCollected { get; set; }

        public void update() { }
    }
}
