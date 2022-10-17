using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeGame
{
    public static class ScalingHelper
    {
        public static double GetGameObjectScale(double windowWidth)
        {
            return windowWidth switch
            {
                <= 300 => 0.60,
                <= 400 => 0.65,
                <= 500 => 0.70,
                <= 700 => 0.75,
                <= 900 => 0.80,
                <= 1000 => 0.85,
                <= 1400 => 0.90,
                <= 2000 => 0.95,
                _ => 1,
            };
        }
    }
}
