using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnakeGame
{
    public class Player : GameObject
    {
        public Player(double size)
        {
            Tag = ElementType.PLAYER;
            CornerRadius = new CornerRadius(5);

            Background = Application.Current.Resources["SnakeBodyColor"] as SolidColorBrush;
            BorderBrush = new SolidColorBrush(Colors.Chocolate);
            BorderThickness = new Thickness(5);

            Width = size;
            Height = size;

            SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER).Value);
        }

        public MovementDirection MovementDirection { get; set; }

        internal void UpdateMovementDirection(MovementDirection movementDirection)
        {
            switch (movementDirection)
            {
                case MovementDirection.Up:
                    if (MovementDirection != MovementDirection.Down)
                    {
                        MovementDirection = MovementDirection.Up;
                        SetDirection(movementDirection);
                        BorderThickness = new Thickness(5, 5, 5, 0);
                    }
                    break;
                case MovementDirection.Left:
                    if (MovementDirection != MovementDirection.Right)
                    {
                        MovementDirection = MovementDirection.Left;
                        SetDirection(movementDirection);
                        BorderThickness = new Thickness(0, 5, 5, 5);
                    }
                    break;
                case MovementDirection.Down:
                    if (MovementDirection != MovementDirection.Up)
                    {
                        MovementDirection = MovementDirection.Down;
                        SetDirection(movementDirection);
                        BorderThickness = new Thickness(5, 0, 5, 5);
                    }
                    break;
                case MovementDirection.Right:
                    if (MovementDirection != MovementDirection.Left)
                    {
                        MovementDirection = MovementDirection.Right;
                        SetDirection(movementDirection);
                        BorderThickness = new Thickness(0, 5, 5, 5);
                    }
                    break;
            }
        }
    }
}
