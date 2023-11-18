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
            public const string UserEndPointExist = UsersEndpoint + "checkExist";
            public const string UserEndPointGetCurrentUser = UsersEndpoint + "/getcurrentuser";
            public const string UserEndPointRegister = UsersEndpoint + "/register";
        }
        public static class Authentication
        {
            public const string AuthenticationEndpoint = ApiEndpoint + "/auth";
            public const string Login = AuthenticationEndpoint + "/login";
            public const string AuthenticationEndpointRefreshToken = ApiEndpoint + "/auth/refreshtoken";

        }
        public static class Class
        {
            public const string ClassEnpoint = ApiEndpoint + "/classes";
            public const string SearchClass = ClassEnpoint + "/search";
            public const string ClassById = ClassEnpoint + "/{id}";
            public const string ClassByCourseId = ClassEnpoint + "/course/{id}";
            public const string FilterClass = SearchClass + "/search/filter";
        }
        public static class Course
        {
            public const string CourseEnpoint = ApiEndpoint + "/Courses";
            public const string SearchCourse = CourseEnpoint + "/search";
            public const string CourseById = ApiEndpoint + "/{id}";
            public const string FilterCourse = SearchCourse + "/filter";
        }
    }
}
