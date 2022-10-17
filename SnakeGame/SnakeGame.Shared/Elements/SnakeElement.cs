using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeGame
{
    public class SnakeElement : GameObject
    {
        #region Properties

        private bool _IsHead;

        public bool IsHead
        {
            get { return _IsHead; }
            set
            {
                _IsHead = value;
                //Background = _IsHead ? new SolidColorBrush(Colors.Purple) : new SolidColorBrush(Colors.Green);
            }
        }


        #endregion

        #region Ctor

        public SnakeElement(double size)
        {
            Child = new Image() { Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/player.gif")) };
            Width = size;
            Height = size;
        }

        #endregion
    }
}
