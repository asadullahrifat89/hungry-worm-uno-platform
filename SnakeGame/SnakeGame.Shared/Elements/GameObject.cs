using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;
using Windows.Foundation;

namespace SnakeGame
{
    public class GameObject : Border
    {
        #region Fields

        private Image _content = new() { Stretch = Stretch.Uniform, Visibility = Microsoft.UI.Xaml.Visibility.Collapsed };

        //private Border _hitBoxborder;

        #endregion

        #region Ctor

        public GameObject()
        {
            Child = _content;
            RenderTransformOrigin = new Point(0.5, 0.5);

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

        #region Properties

        public double Speed { get; set; } = 0;

        public bool IsCollidable { get; set; } = false;

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

        public void SetHitBoxBorder(Rect rect)
        {
            //_hitBoxborder.Height = rect.Height;
            //_hitBoxborder.Width = rect.Width;
        }

        #endregion
    }

    public enum ElementType
    {
        NONE,
        PLAYER,
        PLAYER_POWER_MODE,
        CAR,
        POWERUP,
        HEALTH,        
        CLOUD,
        ISLAND,
        COLLECTIBLE,
    }
}

