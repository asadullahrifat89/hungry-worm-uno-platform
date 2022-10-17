using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeGame
{
    public class Apple : GameObject
    {
        #region Ctor

        public Apple(double size)
        {
            Background = new SolidColorBrush(Colors.Crimson);
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(50);
            //Child = new Image() { Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/collectible.gif")) };
            Height = size;
            Width = size;
        } 

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            if (obj is Apple apple)
                return X == apple.X && Y == apple.Y;
            else
                return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => base.ToString(); 

        #endregion
    }
}
