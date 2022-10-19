using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace HungryWormGame
{
    public class PlayerTrail : GameObject
    {
        #region Fields

        private readonly double _thickness;

        #endregion

        #region Ctor

        public PlayerTrail(double scale)
        {
            Tag = ElementType.PLAYER_TRAIL;
            CornerRadius = new CornerRadius(5);

            Background = Application.Current.Resources["WormBodyColor"] as SolidColorBrush;
            BorderBrush = Application.Current.Resources["WormBorderColor"] as SolidColorBrush;

            Width = Constants.PLAYER_TRAIL_SIZE * scale;
            Height = Constants.PLAYER_TRAIL_SIZE * scale;

            _thickness = 5 * scale;
        }

        #endregion

        #region Methods

        internal void UpdateMovementDirection(MovementDirection up)
        {
            switch (up)
            {
                case MovementDirection.Up:
                case MovementDirection.Down:
                    {
                        BorderThickness = new Thickness(_thickness, 0, _thickness, 0);
                    }
                    break;
                case MovementDirection.Left:
                case MovementDirection.Right:
                    {
                        BorderThickness = new Thickness(0, _thickness, 0, _thickness);
                    }
                    break;
            }
        }

        #endregion
    }
}
