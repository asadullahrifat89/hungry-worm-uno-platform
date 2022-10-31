using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Linq;

namespace HungryWormGame
{
    public class Player : GameObject
    {
        #region Fields

        private double _radius;

        #endregion

        #region Ctor

        public Player(double scale)
        {
            Tag = ElementType.PLAYER;

            SetRoundness(scale);

            Background = Application.Current.Resources["WormBodyColor"] as SolidColorBrush;
            BorderBrush = Application.Current.Resources["WormBorderColor"] as SolidColorBrush;

            Width = Constants.PLAYER_SIZE * scale;
            Height = Constants.PLAYER_SIZE * scale;

            SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER).Value);
        }

        #endregion

        #region Methods

        public void SetRoundness(double scale)
        {
            _radius = 10 * scale;
            CornerRadius = new CornerRadius(_radius);
        }

        #endregion
    }
}
