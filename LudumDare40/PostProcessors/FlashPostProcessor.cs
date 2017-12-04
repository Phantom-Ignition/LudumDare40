using System.Collections;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Tweens;

namespace LudumDare40.PostProcessors
{
    public class FlashPostProcessor : PostProcessor
    {
        /// <summary>
        /// the intensity of the flash
        /// </summary>
        /// <value>The size of the letterbox.</value>
        public float flashIntensity
        {
            get { return _flashIntensity; }
            set
            {
                if (_flashIntensity != value)
                {
                    _flashIntensity = value;

                    if (effect != null)
                        _flashIntensityParam.SetValue(_flashIntensity);
                }
            }
        }

        private float _flashIntensity;
        private EffectParameter _flashIntensityParam;
        private bool _isAnimating;

        public FlashPostProcessor(int executionOrder) : base(executionOrder)
        { }

        public override void onAddedToScene()
        {
            effect = scene.content.loadEffect<Effect>("effects/flash.mgfxo");

            _flashIntensityParam = effect.Parameters["Intensity"];
            _flashIntensityParam.SetValue(_flashIntensity);
        }


        /// <summary>
        /// animates the flash
        /// </summary>
        /// <returns>The in.</returns>
        /// <param name="letterboxSize">Letterbox size.</param>
        /// <param name="duration">Duration.</param>
        /// <param name="easeType">Ease type.</param>
        public IEnumerator animate(float fadeDuration, EaseType easeType = EaseType.SineOut)
        {
            // wait for any current animations to complete
            while (_isAnimating)
                yield return null;

            flashIntensity = 1.0f;

            _isAnimating = true;
            var elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                flashIntensity = Lerps.ease(easeType, 1, 0, elapsedTime, fadeDuration);
                yield return null;
            }
            _isAnimating = false;
        }
    }
}
