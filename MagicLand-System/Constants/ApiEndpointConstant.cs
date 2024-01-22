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
            public const string CheckoutClass = UsersEndpoint + "/checkout";
            public const string CheckOutClassByVnpay = UsersEndpoint + "/vnpay/checkout";
            public const string UserEndPointGetLecturer = UsersEndpoint + "/getLecturer";
            public const string UpdateUser = UsersEndpoint + "/update";
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
            public const string GetAllV2 = ApiEndpoint + "/staff/classes";
            public const string ClassByIdV2 = GetAllV2 + "/{id}";
            public const string StudentInClass = GetAll + "/students/{id}";
            public const string InsertAttandance = GetAll + "/insertAttandance";
            public const string AutoCreateClassEndPoint = GetAll + "/autoCreate";

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
            public const string AddCourseFavoriteList = Origin + "/favorite/add";
            public const string GetCart = Origin + "/view";
            public const string GetFavorite = Origin + "/favorite/view";
            public const string DeleteCartItem = Origin + "/item/delete";
            public const string CheckOutCartItem = Origin + "/item/checkout";
            public const string CheckOutCartItemByVnpay = Origin + "/vnpay/item/checkout";
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
            public const string UpdateStudent = StudentsEndpoint + "/update";
            public const string DeleteStudent = StudentsEndpoint + "/{id}/delete";
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
        public static class WalletTransaction
        {
            public const string GetAll = ApiEndpoint + "/walletTransactions";
            public const string TransactionById = GetAll + "/{id}";
            public const string PersonalWallet = GetAll + "/walletBalance";
            public const string GetBillTransactionById = GetAll + "/{id}/bill/status";
        }

        public static class LectureEndPoint
        {
            public const string EndPointBase = ApiEndpoint + "/lectures";
            public const string TakeStudentAttendance = EndPointBase + "/takeStudentAttendance";
            public const string GetStudentAttendance = EndPointBase + "/student/attendance";
        }

        public static class WalletEndPoint
        {
            public const string EndPointBase = ApiEndpoint + "/wallet";
            public const string TopUpWallet = EndPointBase + "/topup";
        }
    }
}
