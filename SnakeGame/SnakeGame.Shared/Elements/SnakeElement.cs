using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeGame
{
    public class SnakeElement : GameObject
    {
        #region Properties
        
        public bool IsHead { get; set; }

        #endregion

        #region Ctor

        public SnakeElement(double size)
        {
            Background = new SolidColorBrush(Colors.Green);
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(15);
            Width = size;
            Height = size;
        } 

        #endregion
    }
}
