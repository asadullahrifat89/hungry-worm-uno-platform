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

            Background = new SolidColorBrush(Colors.Purple);
            CornerRadius = new CornerRadius(50);

            Width = size;
            Height = size;

            //SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER).Value);
        }

        public MovementDirection MovementDirection { get; set; }

        internal void UpdateMovementDirection(MovementDirection up)
        {
            switch (up)
            {
                case MovementDirection.Up:
                    if (MovementDirection != MovementDirection.Down)
                        MovementDirection = MovementDirection.Up;
                    break;
                case MovementDirection.Left:
                    if (MovementDirection != MovementDirection.Right)
                        MovementDirection = MovementDirection.Left;
                    break;
                case MovementDirection.Down:
                    if (MovementDirection != MovementDirection.Up)
                        MovementDirection = MovementDirection.Down;
                    break;
                case MovementDirection.Right:
                    if (MovementDirection != MovementDirection.Left)
                        MovementDirection = MovementDirection.Right;
                    break;
            }
        }
    }   
}
