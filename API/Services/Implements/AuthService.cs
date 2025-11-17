using API.Services.Interfaces;
using API.UnitOfWorks;
using API.UnitOfWorks;

namespace API.Services.Implements
{
    public partial class AuthService : IAuthService
    {
        private readonly IAuthUow _authUow;

        private readonly IConfiguration _configuration;
        public AuthService(IAuthUow authorUow, IConfiguration configuration)
        {
            _authUow = authorUow;
            _configuration = configuration;
        }
    }
}
