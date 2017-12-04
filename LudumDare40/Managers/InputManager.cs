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

        private VirtualButton _takeThrowButton;
        public VirtualButton TakeThrowButton => _takeThrowButton;

        private VirtualButton _rollButton;
        public VirtualButton RollButton => _rollButton;

        private VirtualButton _jumpButton;
        public VirtualButton JumpButton => _jumpButton;

        private VirtualButton _upButton;
        public VirtualButton UpButton => _upButton;

        private VirtualButton _downButton;
        public VirtualButton DownButton => _downButton;

        private VirtualIntegerAxis _movementAxis;
        public VirtualIntegerAxis MovementAxis => _movementAxis;

        private VirtualButton _selectButton;
        public VirtualButton SelectButton => _selectButton;

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

            _takeThrowButton = new VirtualButton();
            _takeThrowButton
                .addKeyboardKey(Keys.S)
                .addGamePadButton(0, Buttons.Y);

            _rollButton = new VirtualButton();
            _rollButton
                .addKeyboardKey(Keys.D)
                .addGamePadButton(0, Buttons.B);

            _jumpButton = new VirtualButton();
            _jumpButton
                .addKeyboardKey(Keys.W)
                .addGamePadButton(0, Buttons.A);

            _upButton = new VirtualButton();
            _upButton
                .addKeyboardKey(Keys.Up)
                .addGamePadButton(0, Buttons.DPadUp);

            _downButton = new VirtualButton();
            _downButton
                .addKeyboardKey(Keys.Down)
                .addGamePadButton(0, Buttons.DPadDown);

            _movementAxis = new VirtualIntegerAxis();
            _movementAxis
                .addKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right)
                .addGamePadLeftStickX()
                .addGamePadDPadLeftRight();

            _selectButton = new VirtualButton();
            _selectButton
                .addKeyboardKey(Keys.Enter)
                .addGamePadButton(0, Buttons.A)
                .addGamePadButton(0, Buttons.Start);
        }

        public bool isMovementAvailable()
        {
            return !IsBusy && !IsLocked;
        }

        public void update()
        { }
    }
}
