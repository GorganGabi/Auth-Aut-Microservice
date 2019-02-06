using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityMicroservice
{ 
    public class UserRepository
    {
        private readonly UserContext _user;

        public UserRepository(UserContext user) => _user = user;
                                
    }
}
