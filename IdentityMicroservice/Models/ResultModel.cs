namespace IdentityMicroservice
{
    public class ResultModel
    {
        public ResultModel(string id, string token, string role, string email)
        {
            Id = id;
            Token = token;
            Role = role;
            Email = email;
        }

        public string Id { get; private set; }

        public string Token { get; private set; }

        public string Role { get; set; }

        public string Email { get; set; }
    }
}
