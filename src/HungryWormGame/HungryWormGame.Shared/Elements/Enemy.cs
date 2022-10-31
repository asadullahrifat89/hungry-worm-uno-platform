using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HungryWormGame
{
    public class Enemy : GameObject
    {
        #region Ctor

        public Enemy(double scale)
        {
            Tag = ElementType.ENEMY;

            Height = Constants.ENEMY_SIZE * scale;
            Width = Constants.ENEMY_SIZE * scale;
        } 

        #endregion

        #region Methods

        public void SetFacingDirectionX(MovementDirectionX movementDirectionX, double scaleX = 1)
        {
            switch (movementDirectionX)
            {
                case MovementDirectionX.Left:
                    SetScaleX(scaleX * -1);
                    break;
                case MovementDirectionX.Right:
                    SetScaleX(scaleX);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }

    public enum MovementDirectionX
    {
        None,
        Left,
        Right,
    }
}
