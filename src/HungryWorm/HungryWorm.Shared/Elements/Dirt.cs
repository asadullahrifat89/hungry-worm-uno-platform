using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;

namespace HungryWorm
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
