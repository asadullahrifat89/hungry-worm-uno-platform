using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
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
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(50);
            Background = new SolidColorBrush(Colors.Crimson);
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
