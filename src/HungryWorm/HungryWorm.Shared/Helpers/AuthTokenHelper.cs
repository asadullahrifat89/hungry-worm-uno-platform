using System;
using System.Collections.Generic;
using System.Text;

namespace HungryWorm
{
    public static class AuthTokenHelper
    {
        #region Properties

        public static AuthToken AuthToken { get; set; }

        #endregion

        #region Methods

        public static bool WillAuthTokenExpireSoon()
        {
            if (DateTime.UtcNow.AddSeconds(20) > AuthToken.ExpiresOn)
                return true;

            return false;
        }       

        #endregion
    }
}
