using System;
using System.Collections.Generic;
using System.Text;

namespace HungryWorm
{
    public static class Constants
    {
        public const string GAME_ID = "hungry-worm";

        #region Measurements

        public const double DEFAULT_FRAME_TIME = 18;

        public const double PLAYER_SIZE = 100;
        public const double PLAYER_TRAIL_SIZE = 100;

        public const double COLLECTIBLE_SIZE = 80;

        public const double HEALTH_WIDTH = 80;
        public const double HEALTH_HEIGHT = 80;

        #endregion

        #region Images

        public static KeyValuePair<ElementType, Uri>[] ELEMENT_TEMPLATES = new KeyValuePair<ElementType, Uri>[]
        {
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player1.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player2.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player3.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player4.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player5.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player6.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player7.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player8.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player9.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player10.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player11.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player12.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible1.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible2.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible3.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible4.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible5.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible6.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER_TRAIL, new Uri("ms-appx:///Assets/Images/player_trail.png")),
        };

        #endregion

        #region Sounds

        public static KeyValuePair<SoundType, string>[] SOUND_TEMPLATES = new KeyValuePair<SoundType, string>[]
        {
            new KeyValuePair<SoundType, string>(SoundType.MENU_SELECT, "Assets/Sounds/menu-select.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.INTRO, "Assets/Sounds/intro1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.INTRO, "Assets/Sounds/intro2.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.BACKGROUND, "Assets/Sounds/background1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.BACKGROUND, "Assets/Sounds/background2.mp3"),
            //new KeyValuePair<SoundType, string>(SoundType.BACKGROUND, "Assets/Sounds/background3.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.GAME_OVER, "Assets/Sounds/game-over.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.POWER_UP, "Assets/Sounds/power-up.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.POWER_DOWN, "Assets/Sounds/power-down.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.HEALTH_GAIN, "Assets/Sounds/health-gain.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.HEALTH_LOSS, "Assets/Sounds/health-loss.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.ATE_FOOD, "Assets/Sounds/ate-food1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.ATE_FOOD, "Assets/Sounds/ate-food2.mp3"),
        };

        #endregion

        #region Web Api Base Urls
#if DEBUG
        public const string GAME_API_BASEURL = "https://localhost:7238";
#else
        public const string GAME_API_BASEURL = "https://selise-space-shooter-backend.seliselocal.com";
#endif
        #endregion

        #region Web Api Endpoints

        public const string Action_Ping = "/api/Query/Ping";

        public const string Action_Authenticate = "/api/Command/Authenticate";
        public const string Action_SignUp = "/api/Command/SignUp";
        public const string Action_SubmitGameScore = "/api/Command/SubmitGameScore";
        public const string Action_GenerateSession = "/api/Command/GenerateSession";
        public const string Action_ValidateSession = "/api/Command/ValidateSession";

        public const string Action_GetGameProfile = "/api/Query/GetGameProfile";
        public const string Action_GetGameProfiles = "/api/Query/GetGameProfiles";
        public const string Action_GetGameScores = "/api/Query/GetGameScores";
        public const string Action_GetUser = "/api/Query/GetUser";

        #endregion

        #region Session Keys

        public const string CACHE_SESSION_KEY = "Session";
        public const string CACHE_LANGUAGE_KEY = "Language";

        #endregion

        #region Cookie Keys

        public const string COOKIE_KEY = "Cookie";
        public const string COOKIE_ACCEPTED_KEY = "Accepted";

        #endregion
    }

    public enum MovementDirection
    {
        None,
        Right,
        Left,
        Up,
        Down
    }

    //public enum SnakeSize
    //{
    //    Thin = 15,
    //    Normal = 25,
    //    Thick = 50
    //};

    //public enum Movingdirection
    //{
    //    Upwards = 8,
    //    Downwards = 2,
    //    Toleft = 4,
    //    Toright = 6
    //};

    ////TimeSpan values
    //public enum GameSpeed
    //{
    //    Fast = 10,
    //    Moderate = 18,
    //    Slow = 50,
    //    DamnSlow = 100
    //};
}
