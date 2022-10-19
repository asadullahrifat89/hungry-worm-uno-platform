using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Linq;

namespace HungryWorm
{
    public class Player : GameObject
    {
        #region Ctor

        public Player(double size)
        {
            Tag = ElementType.PLAYER;
            CornerRadius = new CornerRadius(5);

            Background = Application.Current.Resources["SnakeBodyColor"] as SolidColorBrush;
            BorderBrush = Application.Current.Resources["SnakeBorderColor"] as SolidColorBrush;

            BorderThickness = new Thickness(5);

            Width = size;
            Height = size;

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
                        BorderThickness = new Thickness(5, 5, 5, 0);                        
                    }
                    break;
                case MovementDirection.Left:
                    if (MovementDirection != MovementDirection.Right)
                    {
                        MovementDirection = MovementDirection.Left;
                        BorderThickness = new Thickness(5, 5, 0, 5);
                    }
                    break;
                case MovementDirection.Down:
                    if (MovementDirection != MovementDirection.Up)
                    {
                        MovementDirection = MovementDirection.Down;
                        BorderThickness = new Thickness(5, 0, 5, 5);
                    }
                    break;
                case MovementDirection.Right:
                    if (MovementDirection != MovementDirection.Left)
                    {
                        MovementDirection = MovementDirection.Right;
                        BorderThickness = new Thickness(0, 5, 5, 5);
                    }
                    break;
            }
        }

        #endregion
    }
}
