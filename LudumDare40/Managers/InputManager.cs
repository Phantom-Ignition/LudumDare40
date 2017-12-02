using Microsoft.Xna.Framework.Input;
using Nez;

namespace LudumDare40.Managers
{
    public class InputManager : IUpdatableManager
    {
        private VirtualButton _interactionButton;
        public VirtualButton InteractionButton => _interactionButton;

        private VirtualButton _attackButton;
        public VirtualButton AttackButton => _attackButton;

        private VirtualButton _jumpButton;
        public VirtualButton JumpButton => _jumpButton;

        private VirtualButton _upButton;
        public VirtualButton UpButton => _upButton;

        private VirtualButton _downButton;
        public VirtualButton DownButton => _downButton;

        private VirtualIntegerAxis _movementAxis;
        public VirtualIntegerAxis MovementAxis => _movementAxis;


        
        // Blocks all the interaction stuff
        public bool IsBusy { get; set; }

        // Blocks all the input
        public bool IsLocked { get; set; }

        public InputManager()
        {
            _interactionButton = new VirtualButton();
            _interactionButton.nodes.Add(new VirtualButton.KeyboardKey(Keys.F));


            _attackButton = new VirtualButton();
            _attackButton
                .addKeyboardKey(Keys.A)
                .addGamePadButton(0, Buttons.X);

            _jumpButton = new VirtualButton();
            _jumpButton
                .addKeyboardKey(Keys.S)
                .addGamePadButton(0, Buttons.A);

            _upButton = new VirtualButton();
            _upButton
                .addKeyboardKey(Keys.Up)
                .addGamePadButton(0, Buttons.DPadUp);

            _movementAxis = new VirtualIntegerAxis();
            _movementAxis
                .addKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right)
                .addGamePadDPadLeftRight();
            
        }

        public bool isMovementAvailable()
        {
            return !IsBusy && !IsLocked;
        }

        public void update()
        { }
    }
}
