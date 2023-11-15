namespace MagicLand_System.Constants
{
    public static class ApiEndpointConstant
    {
        static ApiEndpointConstant()
        {
        }
        public const string RootEndPoint = "/api";
        public const string ApiVersion = "/v1";
        public const string ApiEndpoint = RootEndPoint + ApiVersion;
        
        public static class User
        {
            public const string UsersEndpoint = ApiEndpoint + "/users";
            public const string UserEndpoint = UsersEndpoint + "/{id}";
        }

    }
}
