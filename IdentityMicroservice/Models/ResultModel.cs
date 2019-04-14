namespace IdentityMicroservice
{
    public class ResultModel
    {
        public ResultModel(string id, string token, string role)
        {
            Id = id;
            Token = token;
            Role = role;
        }

        public string Id { get; private set; }

        public string Token { get; private set; }

        public string Role { get; set; }
    }
}
