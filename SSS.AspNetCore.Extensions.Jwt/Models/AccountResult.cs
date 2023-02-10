namespace SSS.AspNetCore.Extensions.Jwt.Models
{
    public class AccountResult
    {
        public AccountResult() { }

        public AccountResult(TokenRequest tokenRequest)
        {
            Successed = true;
            TokenRequest = tokenRequest;
        }

        public AccountResult(object error)
        {
            Successed = false;
            Error = error;
        }

        public bool Successed { get; set; } = true;
        public TokenRequest TokenRequest { get; set; }
        public object Error { get; set; }
    }
}
