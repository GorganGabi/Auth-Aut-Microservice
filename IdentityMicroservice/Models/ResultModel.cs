using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityMicroservice
{
    public class ResultModel
    {
        public ResultModel(string id, string token)
        {
            Id = id;
            Token = token;
        }

        public string Id { get; private set; }

        public string Token { get; private set; }
    }
}
