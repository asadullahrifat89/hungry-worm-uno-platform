using System.Linq;

namespace HungryWormGame
{
    public class PowerUp : GameObject
    {
        //public PowerUpType PowerUpType { get; set; }

        public PowerUp(double size)
        {
            Tag = ElementType.POWERUP;

            Width = size;
            Height = size;            

            SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key == ElementType.POWERUP).Value);
        }
    }

    //public enum PowerUpType
    //{
    //    NONE,
    //    FOOD_MAGNET,
    //}
}

