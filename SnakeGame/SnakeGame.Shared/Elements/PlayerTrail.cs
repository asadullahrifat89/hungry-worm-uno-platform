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
    public class PlayerTrail : GameObject
    {
        #region Properties

        //private bool _IsHead;

        //public bool IsHead
        //{
        //    get { return _IsHead; }
        //    set
        //    {
        //        _IsHead = value;
        //        //BorderBrush = _IsHead ? new SolidColorBrush(Colors.Crimson) : new SolidColorBrush(Colors.Goldenrod);

        //        if (_IsHead)
        //        {
        //            //CornerRadius = new Microsoft.UI.Xaml.CornerRadius(0);
        //            Background = new SolidColorBrush(Colors.Purple);
        //            //SetContent(new Uri("ms-appx:///Assets/Images/character_maleAdventurer_run0.png"));
        //        }
        //        else
        //        {
        //            //HideContent();
        //            Background = new SolidColorBrush(Colors.Goldenrod);
        //            //CornerRadius = new Microsoft.UI.Xaml.CornerRadius(50);
        //        }
        //    }
        //}

        #endregion

        #region Ctor

        public PlayerTrail(double size)
        {
            Tag = ElementType.PLAYER_TRAIL;

            Background = new SolidColorBrush(Colors.Crimson);
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(50);
            BorderBrush = new SolidColorBrush(Colors.Transparent);
            //BorderThickness = new Microsoft.UI.Xaml.Thickness(0, 20, 0, 20);

            Width = size;
            Height = size;
        }

        #endregion
    }
}
