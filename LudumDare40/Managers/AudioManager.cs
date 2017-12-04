using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Audio;

namespace LudumDare40.Managers
{
    public static class AudioManager
    {
        public static AudioSource swordSounds;
        public static SoundEffect alarm;
        public static SoundEffect ambience;
        public static SoundEffect cannon;
        public static SoundEffect electric;
        public static SoundEffect equip;
        public static SoundEffect explosion;
        public static SoundEffect footstep;
        public static AudioSource hit;
        public static AudioSource hitPlayer;
        public static SoundEffect jump;
        public static SoundEffect roll;
        public static SoundEffect select;
        public static SoundEffect shot;
        public static SoundEffect switchSe;

        public static void loadAllSounds()
        {
            swordSounds = new AudioSource();
            swordSounds.addSoundEffect(load(Content.Audios.sword1));
            swordSounds.addSoundEffect(load(Content.Audios.sword2));
            swordSounds.addSoundEffect(load(Content.Audios.sword3));
            swordSounds.setPitchRange(0.1f, 0.9f);
            alarm = load(Content.Audios.alarm);
            ambience = load(Content.Audios.ambience);
            cannon = load(Content.Audios.cannon);
            electric = load(Content.Audios.electric);
            equip = load(Content.Audios.equip);
            explosion = load(Content.Audios.explosion);
            footstep = load(Content.Audios.footstep);
            hit = new AudioSource();
            hit.addSoundEffect(load(Content.Audios.hit1));
            hit.addSoundEffect(load(Content.Audios.hit2));
            hit.addSoundEffect(load(Content.Audios.hit3));
            hitPlayer = new AudioSource();
            hitPlayer.addSoundEffect(load(Content.Audios.hitPlayer1));
            hitPlayer.addSoundEffect(load(Content.Audios.hitPlayer2));
            hitPlayer.addSoundEffect(load(Content.Audios.hitPlayer3));
            jump = load(Content.Audios.jump);
            roll = load(Content.Audios.roll);
            select = load(Content.Audios.select);
            shot = load(Content.Audios.shot);
            switchSe = load(Content.Audios.switchSe);
        }

        private static SoundEffect load(string name)
        {
            return Core.content.Load<SoundEffect>(name);
        }
    }
}
