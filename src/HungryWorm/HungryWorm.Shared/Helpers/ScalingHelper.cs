namespace HungryWorm
{
    public static class ScalingHelper
    {
        public static double GetGameObjectScale(double windowWidth)
        {
            return windowWidth switch
            {
                <= 300 => 0.50,
                <= 400 => 0.55,
                <= 500 => 0.60,
                <= 700 => 0.65,
                <= 900 => 0.70,
                <= 1000 => 0.75,
                <= 1400 => 0.90,
                <= 2000 => 0.95,
                _ => 1,
            };
        }
    }
}
