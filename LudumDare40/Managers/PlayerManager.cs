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
                    if (!FirstCoreCollected)
                        FirstCoreCollected = true;
                    AudioManager.equip.Play();
                }
            }
        }

        public bool FirstCoreCollected { get; set; }
        public int CoresCollected { get; set; }

        public void update() { }
    }
}
