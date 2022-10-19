using System.Linq;

namespace HungryWormGame
{
    public class PowerUp : GameObject
    {
        //public PowerUpType PowerUpType { get; set; }

        public PowerUp(double scale)
        {
            Tag = ElementType.POWERUP;

            Width = Constants.POWERUP_SIZE * scale;
            Height = Constants.POWERUP_SIZE * scale;            

            SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key == ElementType.POWERUP).Value);
        }
    }

    //public enum PowerUpType
    //{
    //    NONE,
    //    FOOD_MAGNET,
    //}
}

