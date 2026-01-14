using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public class TokenSettings
    {
        public string Secret { get; set; }
        public int TokenExpiryInMinutes { get; set; }
        public int RefreshTokenExpiryInDays { get; set; }
    }
}
