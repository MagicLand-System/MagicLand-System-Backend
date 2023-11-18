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
