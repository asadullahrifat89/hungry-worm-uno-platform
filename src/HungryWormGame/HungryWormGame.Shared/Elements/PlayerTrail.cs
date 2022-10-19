using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace HungryWormGame
{
    public class PlayerTrail : GameObject
    {
        #region Fields

        private readonly double _thickness;
        private readonly double _radius;

        #endregion

        #region Ctor

        public PlayerTrail(double scale)
        {
            Tag = ElementType.PLAYER_TRAIL;

            _radius = 5 * scale;
            _thickness = 5 * scale;

            CornerRadius = new CornerRadius(_radius);

            Background = Application.Current.Resources["WormBodyColor"] as SolidColorBrush;
            BorderBrush = Application.Current.Resources["WormBorderColor"] as SolidColorBrush;

            Width = Constants.PLAYER_TRAIL_SIZE * scale;
            Height = Constants.PLAYER_TRAIL_SIZE * scale;
        }

        #endregion

        #region Methods

        internal void UpdateMovementDirection(MovementDirection up)
        {
            switch (up)
            {
                case MovementDirection.Up:
                    {
                        BorderThickness = new Thickness(
                           left: _thickness,
                           top: 0,
                           right: _thickness,
                           bottom: 0);
                    }
                    break;
                case MovementDirection.Down:
                    {
                        BorderThickness = new Thickness(
                            left: _thickness,
                            top: 0,
                            right: _thickness,
                            bottom: 0);
                    }
                    break;
                case MovementDirection.Left:
                    {
                        BorderThickness = new Thickness(
                           left: 0,
                           top: _thickness,
                           right: 0,
                           bottom: _thickness);
                    }
                    break;
                case MovementDirection.Right:
                    {
                        BorderThickness = new Thickness(
                            left: 0,
                            top: _thickness,
                            right: 0,
                            bottom: _thickness);
                    }
                    break;
            }
        }

        #endregion
    }
}
