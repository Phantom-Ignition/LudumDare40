using Microsoft.Xna.Framework.Audio;

namespace LudumDare40.Extensions
{
    public static class SoundEffectExtensions
    {
        public static void Play(this SoundEffect soundEffect, float volume)
        {
            soundEffect.Play(volume, 0.0f, 0.0f);
        }
    }
}
