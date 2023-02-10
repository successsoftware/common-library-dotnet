namespace SSS.AspNetCore.Extensions.Jwt.Models
{
    public class JwtResult
    {
        public JwtResult() { }

        public JwtResult(bool success, object result)
        {
            Success = success;
            Result = result;
        }

        public static JwtResult Failure(object error)
        {
            return new JwtResult { Success = false, Result = error };
        }

        public bool Success { get; set; } = true;
        public object Result { get; set; }
    }
}
