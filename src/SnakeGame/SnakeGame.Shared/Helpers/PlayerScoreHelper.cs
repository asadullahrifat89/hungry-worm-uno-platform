using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeGame
{
    public static class PlayerScoreHelper
    {
        public static SnakeGameScore PlayerScore { get; set; }

        public static bool GameScoreSubmissionPending { get; set; }
    }
}
