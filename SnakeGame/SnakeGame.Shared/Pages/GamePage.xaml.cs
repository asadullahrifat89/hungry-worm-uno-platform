using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;

namespace SnakeGame
{
    public sealed partial class GamePage : Page
    {
        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();

        private int _gameSpeed;
        private readonly int _defaultGameSpeed = 5;

        private bool _isGameOver;
        private bool _isPointerActivated;


        private double _windowHeight, _windowWidth;
        private double _scale;
        private Point _pointerPosition;

        int apples, score, level;

        #endregion

        #region Properties

        public int ElementSize { get; set; } = 50;

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
            }
            if (e.Key == VirtualKey.Right)
            {
                UpdateMovementDirection(MovementDirection.Right);
            }
            if (e.Key == VirtualKey.Up)
            {
                UpdateMovementDirection(MovementDirection.Up);
            }
            if (e.Key == VirtualKey.Down)
            {
                UpdateMovementDirection(MovementDirection.Down);
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

            _gameSpeed = _defaultGameSpeed;

            ResetControls();

            _isGameOver = false;

            score = 0;
            scoreText.Text = "0";

            //foreach (GameObject x in GameView.Children.OfType<GameObject>())
            //{
            //    GameView.AddDestroyableGameObject(x);
            //}

            //RemoveGameObjects();
            InitializeSnake();
            StartGameSounds();
            RunGame();
        }

        private void InitializeSnake()
        {
            Snake = new Snake(ElementSize, _gameSpeed);
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
            {
                StopGame();
            }
        }

        private void ProcessCollisionWithApple()
        {
            IncrementScore();
            GameView.Children.Remove(Apple);
            Apple = null;
            Snake.Grow();
            IncreaseGameSpeed();
        }

        private void IncreaseGameSpeed()
        {
            _gameSpeed++;
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
                X = _random.Next(0, (int)_windowWidth) * ElementSize,
                Y = _random.Next(0, (int)_windowHeight) * ElementSize
            };
        }

        private bool CollisionWithApple()
        {
            if (Apple == null || Snake == null || Snake.Head == null)
                return false;

            SnakeElement head = Snake.Head;
            return (head.X == Apple.X && head.Y == Apple.Y);
        }

        private bool CollisionWithWorldBounds()
        {
            if (Snake == null || Snake.Head == null)
                return false;

            var snakeHead = Snake.Head;

            return (snakeHead.X > _windowWidth - ElementSize ||
                snakeHead.Y > _windowHeight - ElementSize ||
                snakeHead.X < 0 || snakeHead.Y < 0);
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
    }
}
