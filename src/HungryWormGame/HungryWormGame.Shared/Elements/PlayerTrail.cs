using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace HungryWormGame
{
    public class PlayerTrail : GameObject
    {
        #region Fields

        private double _thickness;
        private double _radius;

        #endregion

        #region Ctor

        public PlayerTrail(double scale)
        {
            Tag = ElementType.PLAYER_TRAIL;

            SetRoundness(scale);

            Background = Application.Current.Resources["WormBodyColor"] as SolidColorBrush;
            //BorderBrush = Application.Current.Resources["WormBorderColor"] as SolidColorBrush;

            Width = Constants.PLAYER_TRAIL_SIZE * scale;
            Height = Constants.PLAYER_TRAIL_SIZE * scale;
        }

        #endregion

        #region Methods

        public void SetRoundness(double scale)
        {
            //_thickness = 5 * scale;
            _radius = 50 * scale;

            CornerRadius = new CornerRadius(_radius);
            //BorderThickness = new Thickness(_thickness);
        }

        internal void UpdateMovementDirection(MovementDirection up)
        {
            //switch (up)
            //{
            //    case MovementDirection.Up:
            //        {
            //            BorderThickness = new Thickness(
            //               left: _thickness,
            //               top: 0,
            //               right: _thickness,
            //               bottom: 0);
            //            CornerRadius = new CornerRadius(
            //              topLeft: _radius,
            //              topRight: _radius,
            //              bottomRight: 0,
            //              bottomLeft: 0);
            //        }
            //        break;
            //    case MovementDirection.Down:
            //        {
            //            BorderThickness = new Thickness(
            //                left: _thickness,
            //                top: 0,
            //                right: _thickness,
            //                bottom: 0);
            //            CornerRadius = new CornerRadius(
            //              topLeft: 0,
            //              topRight: 0,
            //              bottomRight: _radius,
            //              bottomLeft: _radius);
            //        }
            //        break;
            //    case MovementDirection.Left:
            //        {
            //            BorderThickness = new Thickness(
            //               left: 0,
            //               top: _thickness,
            //               right: 0,
            //               bottom: _thickness);
            //            CornerRadius = new CornerRadius(
            //             topLeft: _radius,
            //             topRight: 0,
            //             bottomRight: 0,
            //             bottomLeft: _radius);
            //        }
            //        break;
            //    case MovementDirection.Right:
            //        {
            //            BorderThickness = new Thickness(
            //                left: 0,
            //                top: _thickness,
            //                right: 0,
            //                bottom: _thickness);
            //            CornerRadius = new CornerRadius(
            //               topLeft: 0,
            //               topRight: _radius,
            //               bottomRight: _radius,
            //               bottomLeft: 0);
            //        }
            //        break;
            //}
        }

        #endregion
    }
}
