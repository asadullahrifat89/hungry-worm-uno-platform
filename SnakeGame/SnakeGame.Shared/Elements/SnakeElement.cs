using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;

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
                BorderBrush = _IsHead ? new SolidColorBrush(Colors.Crimson) : new SolidColorBrush(Colors.Goldenrod);                

                //if (_IsHead)
                //    SetContent(new Uri("ms-appx:///Assets/Images/character_maleAdventurer_run0.png"));
                //else                
                //    Background = new SolidColorBrush(Colors.Crimson);
                
                //character_maleAdventurer_run0
            }
        }

        #endregion

        #region Ctor

        public SnakeElement(double size)
        {
            Background = new SolidColorBrush(Colors.Goldenrod);
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(50);
            //Child = new Image() { Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/collectible.gif")) };
            Width = size;
            Height = size;
        }

        #endregion
    }
}
