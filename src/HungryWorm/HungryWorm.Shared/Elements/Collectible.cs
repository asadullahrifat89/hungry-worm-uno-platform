using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HungryWorm
{
    public class Collectible : GameObject
    {
        #region Ctor

        public Collectible(double size)
        {
            Tag = ElementType.COLLECTIBLE;

            Height = size;
            Width = size;
        } 

        #endregion
    }
}
