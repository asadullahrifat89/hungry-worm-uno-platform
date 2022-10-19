namespace HungryWormGame
{
    public class Collectible : GameObject
    {
        #region Ctor

        public Collectible(double scale)
        {
            Tag = ElementType.COLLECTIBLE;

            Height = Constants.COLLECTIBLE_SIZE * scale;
            Width = Constants.COLLECTIBLE_SIZE * scale;
        }

        #endregion
    }
}
