using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Linq;

namespace HungryWormGame
{
    public class Player : GameObject
    {
        #region Fields

        private readonly double _thickness;
        private readonly double _radius;

        #endregion

        #region Ctor

        public Player(double scale)
        {
            Tag = ElementType.PLAYER;

            _thickness = 5 * scale;
            _radius = 5 * scale;

            CornerRadius = new CornerRadius(_radius);

            Background = Application.Current.Resources["WormBodyColor"] as SolidColorBrush;
            BorderBrush = Application.Current.Resources["WormBorderColor"] as SolidColorBrush;

            BorderThickness = new Thickness(_thickness);

            Width = Constants.PLAYER_SIZE * scale;
            Height = Constants.PLAYER_SIZE * scale;

            SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER).Value);
        }

        #endregion

        #region Methods

        public void UpdateMovementDirection(MovementDirection movementDirection)
        {
            switch (movementDirection)
            {
                case MovementDirection.Up:
                    if (MovementDirection != MovementDirection.Down)
                    {
                        MovementDirection = MovementDirection.Up;
                        BorderThickness = new Thickness(_thickness, _thickness, _thickness, 0);
                        CornerRadius = new CornerRadius(_radius, _radius, 0, 0);
                    }
                    break;
                case MovementDirection.Left:
                    if (MovementDirection != MovementDirection.Right)
                    {
                        MovementDirection = MovementDirection.Left;
                        BorderThickness = new Thickness(_thickness, _thickness, 0, _thickness);
                        CornerRadius = new CornerRadius(_radius, 0, 0, _radius);

                    }
                    break;
                case MovementDirection.Down:
                    if (MovementDirection != MovementDirection.Up)
                    {
                        MovementDirection = MovementDirection.Down;
                        BorderThickness = new Thickness(_thickness, 0, _thickness, _thickness);
                        CornerRadius = new CornerRadius(0, 0, _radius, _radius);
                    }
                    break;
                case MovementDirection.Right:
                    if (MovementDirection != MovementDirection.Left)
                    {
                        MovementDirection = MovementDirection.Right;
                        BorderThickness = new Thickness(0, _thickness, _thickness, _thickness);
                        CornerRadius = new CornerRadius(0, _radius, _radius, 0);
                    }
                    break;
            }
        }

        #endregion
    }
}
