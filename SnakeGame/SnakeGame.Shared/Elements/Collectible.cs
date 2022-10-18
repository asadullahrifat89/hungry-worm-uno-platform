using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeGame
{
    public class Collectible : GameObject
    {
        #region Ctor

        public Collectible(double size)
        {
            Tag = ElementType.COLLECTIBLE;

            //MovementDirection = (MovementDirection)new Random().Next(1, Enum.GetNames<MovementDirection>().Length);

            Height = size;
            Width = size;
        } 

        #endregion

        #region Methods

        //public override bool Equals(object obj)
        //{
        //    if (obj is Collectible apple)
        //        return X == apple.X && Y == apple.Y;
        //    else
        //        return false;
        //}

        //public override int GetHashCode() => base.GetHashCode();

        //public override string ToString() => base.ToString(); 

        #endregion
    }
}
