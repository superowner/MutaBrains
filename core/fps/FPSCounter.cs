using System;

namespace MutaBrains.Core.FPS
{
    static class FPSCounter
    {
        private static double time;
        private static double frames;

        public static int FPS { get; private set; }
        public static int Max { get; private set; } = int.MinValue;
        public static int Min { get; private set; } = int.MaxValue;

        public static void Calculate(double time)
        {
            FPSCounter.time += time;
            if (FPSCounter.time < 1.0)
            {
                frames++;
            }
            else
            {
                FPS = (int)Math.Ceiling(frames);
                FPSCounter.time = 0.0;
                frames = 0.0;

                if (FPS > Max) Max = FPS;
                if (FPS < Min) Min = FPS;
            }
        }
    }
}
