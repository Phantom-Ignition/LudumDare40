﻿using System.Collections.Generic;

namespace LudumDare40.Components.Sprites
{
    public class FramesList
    {
        public float Delay { get; set; }
        public List<FrameInfo> Frames { get; set; }
        public bool Loop { get; set; }
        public bool Reset { get; set; }
        public List<int> FramesToAttack { get; set; }

        public FramesList(float delay)
        {
            Frames = new List<FrameInfo>();
            FramesToAttack = new List<int>();

            Delay = delay;
            Loop = Delay > 0;
            Reset = Loop;
        }
    }
}
