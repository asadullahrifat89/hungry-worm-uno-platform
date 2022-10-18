using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeGame
{
    public class Dirt : GameObject
    {
        public Dirt(double size)
        {
            Tag = ElementType.DIRT;

            Width = size;
            Height = size;

            CornerRadius = new CornerRadius(size);

            var template = new Random().Next(1, 3);
            Background = Application.Current.Resources[$"DirtBackgroundColor{template}"] as SolidColorBrush;
        }
    }
}
