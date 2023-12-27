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
            public const string UserEndPointCheckoutNow = UsersEndpoint + "/checkoutnow";
            public const string UserEndPointGetLecturer = UsersEndpoint + "/getLecturer";
        }
        public static class Authentication
        {
            public const string AuthenticationEndpoint = ApiEndpoint + "/auth";
            public const string Login = AuthenticationEndpoint + "/login";
            public const string AuthenticationEndpointRefreshToken = ApiEndpoint + "/auth/refreshtoken";

        }
        public static class ClassEnpoint
        {
            public const string GetAll = ApiEndpoint + "/classes";
            public const string ClassById = GetAll + "/{id}";
            public const string ClassByCourseId = GetAll + "/course/{id}";
            public const string FilterClass = GetAll + "/filter";
            public const string AddClass = GetAll + "/add";
        }
        public static class PromotionEnpoint
        {
            public const string GetAll = ApiEndpoint + "/promotion";
            public const string GetCurrent = GetAll + "/currentUser";
        }

        public static class CartEnpoint
        {
            public const string Origin = ApiEndpoint + "/cart";
            public const string ModifyCart = Origin + "/modify";
            public const string GetCart = Origin + "/view";
            public const string DeleteCartItem = Origin + "/item/{id}/delete";
            public const string CheckOutCart = Origin + "/item/checkout";
        }
        public static class CourseEnpoint
        {
            public const string GetAll = ApiEndpoint + "/courses";
            public const string SearchCourse = GetAll + "/search";
            public const string CourseById = GetAll + "/{id}";
            public const string FilterCourse = GetAll + "/filter";
            public const string GetCourseCategory = GetAll + "/categories";
        }
        public static class StudentEndpoint
        {
            public const string StudentsEndpoint = ApiEndpoint + "/students";
            public const string StudentEndpointGet = StudentsEndpoint + "/{id}";
            public const string StudentEnpointCreate = StudentsEndpoint + "/add";
            public const string StudentEndpointGetClass = StudentsEndpoint + "/getclass";
            public const string StudentGetSchedule = StudentsEndpoint + "/getschedule";
            public const string StudentGetCurrentChildren = StudentsEndpoint + "/currentuser";
        }
        public static class RoomEnpoint
        {
            public const string GetAll = ApiEndpoint + "/rooms";
            public const string RoomById = GetAll + "/{id}";
        }
        public static class SlotEnpoint
        {
            public const string GetAll = ApiEndpoint + "/slots";
            public const string SlotById = GetAll + "/{id}";
        }
    }
}
