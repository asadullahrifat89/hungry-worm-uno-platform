using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;

namespace HungryWormGame
{
    public class Spot : GameObject
    {
        public Spot(double size)
        {
            Tag = ElementType.SPOT;

            Width = size;
            Height = size;

            CornerRadius = new CornerRadius(size);

            var template = new Random().Next(1, 3);
            Background = Application.Current.Resources[$"SpotBackgroundColor{template}"] as SolidColorBrush;
        }
    }
}
