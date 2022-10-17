using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Uno;
using Windows.Foundation;
using Windows.System;

namespace SnakeGame
{
    public sealed partial class GamePage : Page
    {
        #region First

        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();

        private bool _isGameOver;
        private bool _isPointerActivated;

        private double _windowHeight, _windowWidth;
        private double _scale;
        private Point _pointerPosition;

        int apples, score, level;

        #endregion

        #region Properties

        public int ElementSize { get; set; } = 80;

        public Apple Apple { get; set; }

        public Snake Snake { get; set; }

        #endregion

        #region Ctor

        public GamePage()
        {
            InitializeComponent();

            _isGameOver = true;
            ShowInGameTextMessage("TAP_ON_SCREEN_TO_BEGIN");

            SoundHelper.LoadGameSounds();

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            Loaded += GamePage_Loaded;
            Unloaded += GamePage_Unloaded;
        }

        #endregion

        #region Events

        #region Page

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += GamePage_SizeChanged;
        }

        private void GamePage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= GamePage_SizeChanged;
            StopGame();
        }

        private void GamePage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            _windowWidth = args.NewSize.Width;
            _windowHeight = args.NewSize.Height;

            SetViewSize();

            Console.WriteLine($"WINDOWS SIZE: {_windowWidth}x{_windowHeight}");
        }

        #endregion

        #region Input

        private void InputView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_isGameOver)
            {
                InputView.Focus(FocusState.Programmatic);
                StartGame();
            }
            else
            {
                _isPointerActivated = true;
            }
        }

        private void InputView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isPointerActivated)
            {
                PointerPoint point = e.GetCurrentPoint(GameView);
                _pointerPosition = point.Position;
            }
        }

        private void InputView_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isPointerActivated = false;
            _pointerPosition = null;
        }

        private void OnKeyUP(object sender, KeyRoutedEventArgs e)
        {
            // when the player releases the left or right key it will set the designated boolean to false
            if (e.Key == VirtualKey.Left)
            {
                UpdateMovementDirection(MovementDirection.Left);
                return;
            }
            if (e.Key == VirtualKey.Right)
            {
                UpdateMovementDirection(MovementDirection.Right);
                return;
            }
            if (e.Key == VirtualKey.Up)
            {
                UpdateMovementDirection(MovementDirection.Up);
                return;
            }
            if (e.Key == VirtualKey.Down)
            {
                UpdateMovementDirection(MovementDirection.Down);
                return;
            }
        }

        #endregion

        #region Button

        private void QuitGameButton_Checked(object sender, RoutedEventArgs e)
        {
            PauseGame();
        }

        private void QuitGameButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ResumeGame();
        }

        private void ConfirmQuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            //NavigateToPage(typeof(StartPage));
        }

        #endregion

        #endregion

        #region Methods

        #region Animation

        #region Game

        private void StartGame()
        {
#if DEBUG
            Console.WriteLine("GAME STARTED");
#endif
            HideInGameTextMessage();
            //SoundHelper.PlaySound(SoundType.MENU_SELECT);

            ResetControls();

            _isGameOver = false;

            score = 0;
            scoreText.Text = "0";

            InitializeSnake();
            StartGameSounds();
            RunGame();
        }

        private void InitializeSnake()
        {
            Snake = new Snake(ElementSize);
            Snake.PositionFirstElement(_random.Next(100, (int)_windowWidth), _random.Next(100, (int)_windowHeight), MovementDirection.Right);
        }

        private void RemoveGameObjects()
        {
            GameView.RemoveDestroyableGameObjects();
        }

        private async void RunGame()
        {
            _gameViewTimer = new PeriodicTimer(_frameTime);

            while (await _gameViewTimer.WaitForNextTickAsync())
            {
                GameViewLoop();
            }
        }

        private void GameViewLoop()
        {
            Snake.MoveSnake();
            CheckCollision();
            CreateApple();
            Draw();
        }

        private void Draw()
        {
            DrawSnake();
            DrawApple();
        }

        private void DrawSnake()
        {
            foreach (var snakeElement in Snake.Elements)
            {
                if (!GameView.Children.Contains(snakeElement))
                    GameView.Children.Add(snakeElement);

                Canvas.SetLeft(snakeElement, snakeElement.X);
                Canvas.SetTop(snakeElement, snakeElement.Y);
            }
        }

        private void DrawApple()
        {
            if (!GameView.Children.Contains(Apple))
                GameView.Children.Add(Apple);

            Canvas.SetLeft(Apple, Apple.X);
            Canvas.SetTop(Apple, Apple.Y);
        }

        private void CheckCollision()
        {
            if (CollisionWithApple())
                ProcessCollisionWithApple();

            if (Snake.CollisionWithSelf() || CollisionWithWorldBounds())
                StopGame();
        }

        private void ProcessCollisionWithApple()
        {
            Console.WriteLine("APPLE REMOVED");

            IncrementScore();
            GameView.Children.Remove(Apple);
            Apple = null;
            Snake.Grow();
            //IncreaseGameSpeed();
        }

        private void IncreaseGameSpeed()
        {
            //_gameSpeed++;
        }

        internal void IncrementScore()
        {
            apples += 1;
            if (apples % 3 == 0)
                level += 1;
            score += 10 * level;
            UpdateScore();
        }

        internal void UpdateScore()
        {
            //ApplesLbl.Content = $"Apples: {apples}";
            scoreText.Text = $"{score}";
            //LevelLbl.Content = $"Level: {level}";
        }

        private void CreateApple()
        {
            if (Apple != null)
                return;

            Apple = new Apple(ElementSize)
            {
                X = _random.Next(100, (int)_windowWidth - 100),
                Y = _random.Next(100, (int)_windowHeight - 100),
            };
        }

        private bool CollisionWithApple()
        {
            if (Apple == null || Snake == null || Snake.Head == null)
                return false;

            SnakeElement source = Snake.Head;
            var target = Apple;

            //return (head.X == Apple.X && head.Y == Apple.Y);

            if (source.Width >= 0.0 && source.Width >= 0.0
               && target.X <= source.X + source.Width && target.X + target.Width >= source.X
               && target.Y <= source.Y + source.Height)
            {
                return target.Y + target.Height >= source.Y;
            }

            return false;
        }

        private bool CollisionWithWorldBounds()
        {
            if (Snake == null || Snake.Head == null)
                return false;

            var snakeHead = Snake.Head;

            //return (snakeHead.X > _windowWidth - ElementSize ||
            //    snakeHead.Y > _windowHeight - ElementSize ||
            //    snakeHead.X < 0 || snakeHead.Y < 0);

            if (snakeHead.X > _windowWidth)
            {
                snakeHead.X = 0;
            }

            if (snakeHead.X < 0)
            {
                snakeHead.X = _windowWidth;
            }

            if (snakeHead.Y > _windowHeight)
            {
                snakeHead.Y = 0;
            }

            if (snakeHead.Y < 0)
            {
                snakeHead.Y = _windowHeight;
            }

            return false;
        }

        private void PauseGame()
        {
            InputView.Focus(FocusState.Programmatic);
            ShowInGameTextMessage("GAME_PAUSED");

            _gameViewTimer?.Dispose();

            ResetControls();

            //SoundHelper.PlaySound(SoundType.MENU_SELECT);
            PauseGameSounds();
        }

        private void ResumeGame()
        {
            InputView.Focus(FocusState.Programmatic);
            HideInGameTextMessage();

            //SoundHelper.PlaySound(SoundType.MENU_SELECT);
            //SoundHelper.ResumeSound(SoundType.BACKGROUND);
            //SoundHelper.ResumeSound(SoundType.CAR_ENGINE);

            RunGame();
        }

        private void StopGame()
        {
            _gameViewTimer?.Dispose();
            StopGameSounds();
        }

        private void ResetControls()
        {
            _isPointerActivated = false;
        }

        internal void UpdateMovementDirection(MovementDirection movementDirection)
        {
            if (Snake != null)
                Snake.UpdateMovementDirection(movementDirection);
        }

        #endregion

        #endregion

        #region Sound

        private async void StartGameSounds()
        {
            //SoundHelper.PlaySound(SoundType.CAR_START);

            //await Task.Delay(TimeSpan.FromSeconds(1));

            //SoundHelper.PlaySound(SoundType.CAR_ENGINE);

            //SoundHelper.RandomizeBackgroundSound();
            //SoundHelper.PlaySound(SoundType.BACKGROUND);
        }

        private void StopGameSounds()
        {
            //SoundHelper.StopSound(SoundType.BACKGROUND);
            //SoundHelper.StopSound(SoundType.CAR_ENGINE);
        }

        private void PauseGameSounds()
        {
            //SoundHelper.PauseSound(SoundType.BACKGROUND);
            //SoundHelper.PauseSound(SoundType.CAR_ENGINE);
        }

        #endregion

        #region Page

        private void SetViewSize()
        {
            _scale = ScalingHelper.GetGameObjectScale(_windowWidth);

            GameView.Width = _windowWidth;
            GameView.Height = _windowHeight;
        }

        private void NavigateToPage(Type pageType)
        {
            //SoundHelper.PlaySound(SoundType.MENU_SELECT);
            App.NavigateToPage(pageType);
        }

        #endregion

        #region In Game Message

        private void ShowInGameTextMessage(string resourceKey)
        {
            InGameMessageText.Text = LocalizationHelper.GetLocalizedResource(resourceKey);
            InGameMessagePanel.Visibility = Visibility.Visible;
        }

        private void HideInGameTextMessage()
        {
            InGameMessageText.Text = "";
            InGameMessagePanel.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion

        #endregion

        #region Second

        //#region Fields

        //private bool _isGameOver;
        //private bool _isPointerActivated;

        //// This list describes the Bonus Red pieces of Food on the Canvas
        //private readonly List<Point> _bonusPoints = new List<Point>();

        //// This list describes the body of the snake on the Canvas
        //private readonly List<Point> _snakePoints = new List<Point>();

        //private readonly Brush _snakeColor = new SolidColorBrush(Colors.Green);

        //private readonly Point _startingPoint = new Point(100, 100);
        //private Point _currentPosition = new Point();

        //// Movement direction initialisation
        //private int _direction = 0;

        ///* Placeholder for the previous movement direction
        // * The snake needs this to avoid its own body.  */
        //private int _previousDirection = 0;

        ///* Here user can change the size of the snake. 
        // * Possible sizes are THIN, NORMAL and THICK */
        //private readonly int _headSize = (int)SnakeSize.Thick;

        //private int _length = 100;
        //private int _score = 0;
        //private readonly Random _rnd = new Random();

        //DispatcherTimer _timer;
        //private double _windowHeight, _windowWidth;
        //private double _scale;

        //private Point _pointerPosition;

        //#endregion

        //#region Ctor

        //public GamePage()
        //{
        //    InitializeComponent();

        //    _isGameOver = true;

        //    _windowHeight = Window.Current.Bounds.Height;
        //    _windowWidth = Window.Current.Bounds.Width;

        //    Loaded += GamePage_Loaded;
        //    Unloaded += GamePage_Unloaded;
        //}

        //#endregion

        //#region Events

        //private void GamePage_Loaded(object sender, RoutedEventArgs e)
        //{
        //    SizeChanged += GamePage_SizeChanged;
        //}

        //private void GamePage_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    SizeChanged -= GamePage_SizeChanged;
        //    StopGame();
        //}

        //private void GamePage_SizeChanged(object sender, SizeChangedEventArgs args)
        //{
        //    _windowWidth = args.NewSize.Width;
        //    _windowHeight = args.NewSize.Height;

        //    SetViewSize();

        //    Console.WriteLine($"WINDOWS SIZE: {_windowWidth}x{_windowHeight}");
        //}

        //private void OnKeyUP(object sender, KeyRoutedEventArgs e)
        //{
        //    Console.WriteLine("KEY DOWN");
        //    switch (e.Key)
        //    {
        //        case VirtualKey.Down:
        //            if (_previousDirection != (int)Movingdirection.Upwards)
        //                _direction = (int)Movingdirection.Downwards;
        //            break;
        //        case VirtualKey.Up:
        //            if (_previousDirection != (int)Movingdirection.Downwards)
        //                _direction = (int)Movingdirection.Upwards;
        //            break;
        //        case VirtualKey.Left:
        //            if (_previousDirection != (int)Movingdirection.Toright)
        //                _direction = (int)Movingdirection.Toleft;
        //            break;
        //        case VirtualKey.Right:
        //            if (_previousDirection != (int)Movingdirection.Toleft)
        //                _direction = (int)Movingdirection.Toright;
        //            break;

        //    }

        //    _previousDirection = _direction;
        //}

        //private void InputView_PointerPressed(object sender, PointerRoutedEventArgs e)
        //{
        //    if (_isGameOver)
        //    {
        //        InputView.Focus(FocusState.Programmatic);
        //        StartGame();
        //    }
        //    else
        //    {
        //        _isPointerActivated = true;
        //    }
        //}

        //private void InputView_PointerMoved(object sender, PointerRoutedEventArgs e)
        //{
        //    if (_isPointerActivated)
        //    {
        //        PointerPoint point = e.GetCurrentPoint(GameView);
        //        _pointerPosition = point.Position;
        //    }
        //}

        //private void InputView_PointerReleased(object sender, PointerRoutedEventArgs e)
        //{
        //    _isPointerActivated = false;
        //    _pointerPosition = null;
        //}

        //#endregion

        //#region Methods

        //private void SetViewSize()
        //{
        //    _scale = ScalingHelper.GetGameObjectScale(_windowWidth);

        //    GameView.Width = _windowWidth;
        //    GameView.Height = _windowHeight;
        //}

        //private void StartGame()
        //{
        //    Console.WriteLine("START GAME");
        //    _isGameOver = false;

        //    PaintSnake(_startingPoint);
        //    _currentPosition = _startingPoint;

        //    if (_bonusPoints.Count == 0)
        //    {
        //        // Instantiate Food Objects
        //        for (var n = 0; n < 10; n++)
        //        {
        //            PaintBonus(n);
        //        }
        //    }

        //    _timer = new DispatcherTimer();
        //    _timer.Tick += Timer_Tick;

        //    /* Here user can change the speed of the snake. 
        //     * Possible speeds are FAST, MODERATE, SLOW and DAMNSLOW */
        //    _timer.Interval = TimeSpan.FromMilliseconds((double)GameSpeed.Moderate);
        //    _timer.Start();
        //}

        //private void Timer_Tick(object sender, object e)
        //{
        //    // Expand the body of the snake to the direction of movement

        //    switch (_direction)
        //    {
        //        case (int)Movingdirection.Downwards:
        //            _currentPosition.Y += 1;
        //            PaintSnake(_currentPosition);
        //            break;
        //        case (int)Movingdirection.Upwards:
        //            _currentPosition.Y -= 1;
        //            PaintSnake(_currentPosition);
        //            break;
        //        case (int)Movingdirection.Toleft:
        //            _currentPosition.X -= 1;
        //            PaintSnake(_currentPosition);
        //            break;
        //        case (int)Movingdirection.Toright:
        //            _currentPosition.X += 1;
        //            PaintSnake(_currentPosition);
        //            break;
        //    }

        //    // Restrict to boundaries of the Canvas
        //    if ((_currentPosition.X < 0) || (_currentPosition.X > _windowWidth) ||
        //        (_currentPosition.Y < 0) || (_currentPosition.Y > _windowHeight))
        //        StopGame();

        //    // Hitting a bonus Point causes the lengthen-Snake Effect
        //    int n = 0;
        //    foreach (Point point in _bonusPoints)
        //    {
        //        if ((Math.Abs(point.X - _currentPosition.X) < _headSize) &&
        //            (Math.Abs(point.Y - _currentPosition.Y) < _headSize))
        //        {
        //            _length += 10;
        //            _score += 10;

        //            scoreText.Text = _score.ToString();

        //            // In the case of food consumption, erase the food object
        //            // from the list of bonuses as well as from the canvas
        //            _bonusPoints.RemoveAt(n);
        //            GameView.Children.RemoveAt(n);
        //            PaintBonus(n);
        //            break;
        //        }
        //        n++;
        //    }

        //    // Restrict hits to body of Snake
        //    for (int q = 0; q < (_snakePoints.Count - _headSize * 2); q++)
        //    {
        //        Point point = new Point(_snakePoints[q].X, _snakePoints[q].Y);
        //        if ((Math.Abs(point.X - _currentPosition.X) < (_headSize)) &&
        //             (Math.Abs(point.Y - _currentPosition.Y) < (_headSize)))
        //        {
        //            StopGame();
        //            break;
        //        }
        //    }
        //}

        //private void PaintSnake(Point currentposition)
        //{
        //    // This method is used to paint a frame of the snake´s body each time it is called.

        //    Border newEllipse = new()
        //    {
        //        Child = new Image() { Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/player.gif")) },
        //        Width = _headSize,
        //        Height = _headSize
        //    };

        //    Canvas.SetTop(newEllipse, currentposition.Y);
        //    Canvas.SetLeft(newEllipse, currentposition.X);

        //    int count = GameView.Children.Count;
        //    GameView.Children.Add(newEllipse);
        //    _snakePoints.Add(currentposition);

        //    // Restrict the tail of the snake
        //    if (count > _length)
        //    {
        //        GameView.Children.RemoveAt(count - _length + 9);
        //        _snakePoints.RemoveAt(count - _length);
        //    }
        //}

        //private void PaintBonus(int index)
        //{
        //    Point bonusPoint = new Point(_rnd.Next(5, (int)_windowWidth), _rnd.Next(5, (int)_windowHeight));

        //    Border newEllipse = new()
        //    {
        //        Child = new Image() { Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/collectible.gif")) },
        //        Width = _headSize,
        //        Height = _headSize
        //    };

        //    Canvas.SetTop(newEllipse, bonusPoint.Y);
        //    Canvas.SetLeft(newEllipse, bonusPoint.X);
        //    GameView.Children.Insert(index, newEllipse);
        //    _bonusPoints.Insert(index, bonusPoint);
        //}

        //private void StopGame()
        //{
        //    _timer.Stop();

        //    Console.WriteLine("GAME OVER");

        //    //MessageBox.Show($@"You Lose! Your score is {_score}", "Game Over", MessageBoxButton.OK, MessageBoxImage.Hand);
        //    //this.Close();
        //}

        //#endregion

        #endregion
    }

    public enum SnakeSize
    {
        Thin = 15,
        Normal = 25,
        Thick = 50
    };

    public enum Movingdirection
    {
        Upwards = 8,
        Downwards = 2,
        Toleft = 4,
        Toright = 6
    };

    //TimeSpan values
    public enum GameSpeed
    {
        Fast = 10,
        Moderate = 18,
        Slow = 50,
        DamnSlow = 100
    };
}
