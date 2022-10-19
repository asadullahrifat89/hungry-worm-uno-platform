using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.Foundation;

namespace HungryWormGame
{
    public class GameObject : Border
    {
        #region Fields

        private readonly Image _content = new() { Stretch = Stretch.Uniform, Visibility = Microsoft.UI.Xaml.Visibility.Collapsed };

        private readonly CompositeTransform _compositeTransform = new()
        {
            CenterX = 0.5,
            CenterY = 0.5,
            Rotation = 0,
            ScaleX = 1,
            ScaleY = 1,
        };

        //private Border _hitBoxborder;

        #endregion

        #region Properties

        public MovementDirection MovementDirection { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public bool IsFlaggedForShrinking { get; set; }

        public bool HasShrinked => _compositeTransform.ScaleX <= 0;

        #endregion

        #region Ctor

        public GameObject()
        {
            Child = _content;
            RenderTransformOrigin = new Point(0.5, 0.5);

            RenderTransform = _compositeTransform;
            CanDrag = false;

            #region HitBox Debug

            //BorderThickness = new Microsoft.UI.Xaml.Thickness(1);
            //BorderBrush = new SolidColorBrush(Colors.Black);

            //_hitBoxborder = new Border()
            //{
            //    BorderThickness = new Microsoft.UI.Xaml.Thickness(1),
            //    BorderBrush = new SolidColorBrush(Colors.Black)
            //};


            //var grid = new Grid()
            //{
            //    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
            //    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
            //};

            //grid.Children.Add(_hitBoxborder);
            //grid.Children.Add(_content);
            //Child = grid;

            #endregion
        }

        #endregion        

        #region Methods

        public void SetSize(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public double GetTop()
        {
            return Canvas.GetTop(this);
        }

        public double GetLeft()
        {
            return Canvas.GetLeft(this);
        }

        public void SetTop(double top)
        {
            Canvas.SetTop(this, top);
        }

        public void SetLeft(double left)
        {
            Canvas.SetLeft(this, left);
        }

        public void SetZ(int z)
        {
            Canvas.SetZIndex(this, z);
        }

        public void SetPosition(double left, double top)
        {
            Canvas.SetTop(this, top);
            Canvas.SetLeft(this, left);
        }

        public void SetContent(Uri uri)
        {
            _content.Source = new BitmapImage(uri);
            _content.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }

        public void HideContent()
        {
            _content.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        public void SetHitBoxBorder(Rect rect)
        {
            //_hitBoxborder.Height = rect.Height;
            //_hitBoxborder.Width = rect.Width;
        }

        public void Shrink()
        {
            _compositeTransform.ScaleX -= 0.1;
            _compositeTransform.ScaleY -= 0.1;
        }

        #endregion
    }

    public enum ElementType
    {
        NONE,
        PLAYER,
        PLAYER_TRAIL,
        POWERUP,
        HEALTH,
        COLLECTIBLE,
        DIRT,
    }
}

