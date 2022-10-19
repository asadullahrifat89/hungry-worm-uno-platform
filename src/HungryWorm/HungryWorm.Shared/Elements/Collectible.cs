namespace HungryWorm
{
    public class Collectible : GameObject
    {
        #region Ctor

        public Collectible(double size)
        {
            Tag = ElementType.COLLECTIBLE;

            Height = size;
            Width = size;
        } 

        #endregion
    }
}
