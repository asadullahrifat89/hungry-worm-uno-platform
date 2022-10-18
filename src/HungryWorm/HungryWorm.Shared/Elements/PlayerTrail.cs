using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI;

namespace HungryWorm
{
    public class PlayerTrail : GameObject
    {
        #region Ctor

        public PlayerTrail(double size)
        {
            Tag = ElementType.PLAYER_TRAIL;
            CornerRadius = new CornerRadius(5);

            Background = Application.Current.Resources["SnakeBodyColor"] as SolidColorBrush;
            BorderBrush = Application.Current.Resources["SnakeBorderColor"] as SolidColorBrush;

            Width = size;
            Height = size;
        }

        #endregion

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

        #region Methods

        internal void UpdateMovementDirection(MovementDirection up)
        {
            switch (up)
            {
                case MovementDirection.Up:
                case MovementDirection.Down:
                    {
                        BorderThickness = new Thickness(5, 0, 5, 0);
                    }
                    break;
                case MovementDirection.Left:
                case MovementDirection.Right:
                    {
                        BorderThickness = new Thickness(0, 5, 0, 5);
                    }
                    break;
            }
        }

        #endregion
    }
}
