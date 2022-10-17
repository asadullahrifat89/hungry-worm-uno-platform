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

        private bool _IsHead;

        public bool IsHead
        {
            get { return _IsHead; }
            set
            {
                _IsHead = value;
                Background = _IsHead ? new SolidColorBrush(Colors.Purple) : new SolidColorBrush(Colors.Green);

            }
        }


        #endregion

        #region Ctor

        public SnakeElement(double size)
        {
            Background = new SolidColorBrush(Colors.Green);
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(0);
            Width = size;
            Height = size;
        }

        #endregion
    }
}
