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

            Background = new SolidColorBrush(Colors.Goldenrod);
            CornerRadius = new CornerRadius(50);

            Width = size;
            Height = size;

            //SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER).Value);
        }
    }

    public enum MovementDirection
    {
        Right,
        Left,
        Up,
        Down
    }
}
