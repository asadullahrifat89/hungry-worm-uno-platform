using System;
using System.Collections.Generic;
using System.Text;

namespace HungryWorm
{
    public static class PlayerScoreHelper
    {
        public static HungryWormScore PlayerScore { get; set; }

        public static bool GameScoreSubmissionPending { get; set; }
    }
}
