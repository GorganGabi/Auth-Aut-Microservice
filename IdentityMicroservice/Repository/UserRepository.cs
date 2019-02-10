namespace IdentityMicroservice
{
    public class UserRepository
    {
        private readonly UserContext _user;

        public UserRepository(UserContext user) => _user = user;
                                
    }
}
