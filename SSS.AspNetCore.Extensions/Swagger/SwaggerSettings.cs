namespace SSS.AspNetCore.Extensions.Swagger
{
    public class SwaggerInfo
    {
        public static string Name => "Swagger";

        public string Contact { get; set; }

        public string Description { get; set; }

        public string License { get; set; }

        public string TermsOfService { get; set; }

        public string Title { get; set; }

        public string Version { get; set; }
    }
}