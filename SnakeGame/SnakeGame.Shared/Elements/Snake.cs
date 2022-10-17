using System;
using System.Collections.Generic;
using System.Linq;

namespace SnakeGame
{
    public class Snake
    {
        #region Fields

        private readonly int _elementSize;
        private readonly double _gameSpeed;

        #endregion

        #region Properties

        public SnakeElement TailBackup { get; set; }

        public List<SnakeElement> Elements { get; set; }

        public MovementDirection MovementDirection { get; set; }

        public SnakeElement Head => Elements.Any() ? Elements[0] : null;

        #endregion

        #region Ctor

        public Snake(int elementSize, double gameSpeed)
        {
            Elements = new List<SnakeElement>();
            _elementSize = elementSize;
            _gameSpeed = gameSpeed;
        }

        #endregion

        #region Methods

        internal void UpdateMovementDirection(MovementDirection up)
        {
            switch (up)
            {
                case MovementDirection.Up:
                    if (MovementDirection != MovementDirection.Down)
                        MovementDirection = MovementDirection.Up;
                    break;
                case MovementDirection.Left:
                    if (MovementDirection != MovementDirection.Right)
                        MovementDirection = MovementDirection.Left;
                    break;
                case MovementDirection.Down:
                    if (MovementDirection != MovementDirection.Up)
                        MovementDirection = MovementDirection.Down;
                    break;
                case MovementDirection.Right:
                    if (MovementDirection != MovementDirection.Left)
                        MovementDirection = MovementDirection.Right;
                    break;
            }
        }

        internal void Grow()
        {
            Elements.Add(new SnakeElement(_elementSize) { X = TailBackup.X, Y = TailBackup.Y });
        }

        public bool CollisionWithSelf()
        {
            SnakeElement snakeHead = Head;
            if (snakeHead != null)
            {
                foreach (var snakeElement in Elements)
                {
                    if (!snakeElement.IsHead)
                    {
                        if (snakeElement.X == snakeHead.X && snakeElement.Y == snakeHead.Y)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal void PositionFirstElement(double x, double y, MovementDirection initialDirection)
        {
            Elements.Add(new SnakeElement(_elementSize)
            {
                X = x,
                Y = y,
                IsHead = true
            });
            MovementDirection = initialDirection;
        }

        internal void MoveSnake()
        {
            SnakeElement head = Elements[0];
            SnakeElement tail = Elements[Elements.Count - 1];

            TailBackup = new SnakeElement(_elementSize)
            {
                X = tail.X,
                Y = tail.Y
            };

            head.IsHead = false;
            tail.IsHead = true;

            tail.X = head.X;
            tail.Y = head.Y;

            switch (MovementDirection)
            {
                case MovementDirection.Right:
                    tail.X += _elementSize;
                    break;
                case MovementDirection.Left:
                    tail.X -= _elementSize;
                    break;
                case MovementDirection.Up:
                    tail.Y -= _elementSize;
                    break;
                case MovementDirection.Down:
                    tail.Y += _elementSize;
                    break;
                default:
                    break;
            }
            Elements.RemoveAt(Elements.Count - 1);
            Elements.Insert(0, tail);
        }

        #endregion
    }

    public enum MovementDirection
    {
        Right,
        Left,
        Up,
        Down
    }
}
