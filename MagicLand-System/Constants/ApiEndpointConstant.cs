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
            public const string GetAllClassNotInCart = ApiEndpoint + "/classes/notInCart";
            public const string ClassById = GetAll + "/{id}";
            public const string ClassByCourseId = GetAll + "/course/{id}";
            public const string FilterClass = GetAll + "/filter";
            public const string AddClass = GetAll + "/add";
            public const string GetAllV2 = ApiEndpoint + "/staff/classes";
            public const string ClassByIdV2 = GetAllV2 + "/{id}";
            public const string StudentInClass = GetAll + "/students/{id}";
            public const string InsertAttandance = GetAll + "/insertAttandance";
            public const string AutoCreateClassEndPoint = GetAll + "/autoCreate";
            public const string UpdateClass = ClassById + "/update";
            public const string SessionLoad = GetAll + "/loadSession";
            public const string LoadClassForAttandance = GetAll + "/loadClassForAttedance";
            public const string GetStuitableClass = GetAll + "/staff/change/suitable";
            public const string ChangeClass = GetAll + "/staff/change";
            public const string CancelClass = GetAll + "/cancel/{classId}";
            public const string UpdateSession = ClassById + "/updateSession";
            public const string MakeUpClass = GetAll + "/{studentId}" + "/{scheduleId}/makeup";
            public const string GetMakeUpClass = GetAll + "/getMakeUpSchedule";
            public const string InsertClasses = GetAll + "/insertClasses";
            public const string CheckingClassForStudents = GetAll + "/students/checking";
            public const string GetClassValid = GetAll + "/valid";
            public const string GetTopicLearning = GetAll + "/topic/learning";
            public const string GetStudentValid = GetAll + "/students/valid";
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
            public const string GetAll = Origin + "/items/all";
            public const string CheckOutCartItemByVnpay = Origin + "/vnpay/item/checkout";
        }
        public static class CourseEnpoint
        {
            public const string GetAll = ApiEndpoint + "/courses";
            public const string GetAllValid = ApiEndpoint + "/courses/validRegister";
            public const string GetCurrentStudentCourses = ApiEndpoint + "/courses/currentStudent";
            public const string SearchCourse = GetAll + "/search";
            public const string CourseById = GetAll + "/{id}";
            public const string FilterCourse = GetAll + "/filter";
            public const string GetCourseCategory = GetAll + "/categories";
            public const string AddCourse = GetAll + "/add";
            public const string GetCourseByStaff = GetAll + "/staff/get";
        }
        public static class StudentEndpoint
        {
            public const string StudentsEndpoint = ApiEndpoint + "/students";
            public const string StudentEndpointGet = StudentsEndpoint + "/{id}";
            public const string StudentEnpointCreate = StudentsEndpoint + "/add";
            public const string StudentEndpointGetClass = StudentsEndpoint + "/getclass";
            public const string StudentGetSchedule = StudentsEndpoint + "/getschedule";
            public const string GetStudentAccount = StudentsEndpoint + "/getAccount";
            public const string StudentGetCurrentChildren = StudentsEndpoint + "/currentuser";
            public const string UpdateStudent = StudentsEndpoint + "/update";
            public const string DeleteStudent = StudentsEndpoint + "/{id}/delete";
            public const string GetStudentCourseRegistered = StudentsEndpoint + "/{id}/getcourses";
            public const string GetStatisticRegisterStudent= StudentsEndpoint + "/register/statistic";
        }
        public static class RoomEnpoint
        {
            public const string GetAll = ApiEndpoint + "/rooms";
            public const string RoomById = GetAll + "/{id}";
            public const string RoomByAdmin = GetAll + "/admin/get";
        }
        public static class SlotEnpoint
        {
            public const string GetAll = ApiEndpoint + "/slots";
            public const string SlotById = GetAll + "/{id}";
        }
        public static class WalletTransactionEndPoint
        {
            public const string GetAll = ApiEndpoint + "/walletTransactions";
            public const string TransactionById = GetAll + "/{id}";
            public const string PersonalWallet = GetAll + "/walletBalance";
            public const string GetBillTransactionById = GetAll + "/{id}/bill/status";
            public const string GetBillTransactionByTxnRefCode = GetAll + "/{txnRefCode}/bills/status";
            public const string GetRevenueTransactionByTime = GetAll + "/bills/revenue";
        }
        public static class SyllabusEndPoint
        {
            public const string EndPointBase = ApiEndpoint + "/Syllabus";
            public const string CheckingSyllabusInfor = ApiEndpoint + "/Syllabus/infor/checking";
            public const string LoadByCourse = EndPointBase + "/getByCourse";
            public const string AddSyllabus = EndPointBase + "/insertSyllabus";
            public const string LoadSyllabus = EndPointBase + "/getById";
            public const string LoadSyllabuses = EndPointBase + "/getAll";
            public const string FilterSyllabus = EndPointBase + "/filter";
            public const string GeneralSyllabus = EndPointBase + "/general";
            public const string UpdateSyllabus = EndPointBase + "/{id}/update";
            public const string StaffSyl = EndPointBase + "/{id}/staff/get";
            public const string AvailableSyl = EndPointBase + "/available";
            public const string UpdateOverall = EndPointBase + "/{id}/updateOverall";
            public const string UpdateTopic = EndPointBase + "/{topicId}/updateTopic";
            public const string UpdateSession = EndPointBase + "/{descriptionId}/updateSession";
            public const string GenralInfromation = EndPointBase + "/staff/getGeneralInformation";
            public const string MaterialInfor = EndPointBase + "/staff/getMaterial";
            public const string ExamSyllabus = EndPointBase + "/staff/getExamSyllabus";
            public const string SessionSyllabus = EndPointBase + "/staff/getSessionSyllabus";
            public const string QuestionSyllabus = EndPointBase + "/staff/getQuestionSyllabus";
        }
        public static class LectureEndPoint
        {
            public const string EndPointBase = ApiEndpoint + "/lectures";
            public const string TakeStudentAttendance = EndPointBase + "/students/takeAttendance";
            public const string EvaluateStudent = EndPointBase + "/students/evaluate";
            public const string GetStudentEvaluates = EndPointBase + "/students/get/evaluates";
            public const string GetStudentAttendance = EndPointBase + "/student/attendance";
            public const string GetStudentAttendanceOfAllClass = EndPointBase + "/student/classes/attendance";
            public const string GetCurrentClassesSchedule = EndPointBase + "/current/classes";
            public const string GetClassesAttendanceWithDate = EndPointBase + "/class/date/attendances";
            public const string GetLectureSchedule = EndPointBase + "/schedules";
        }

        public static class AttendanceEndPoint
        {
            public const string EndPointBase = ApiEndpoint + "/attendance";
            public const string GetAttendanceOfClass = EndPointBase + "/class/{id}";
            public const string GetAttendanceOfClasses = EndPointBase + "/classes";
            public const string GetAttendanceOfStudent = EndPointBase + "/student/{id}";
            public const string LoadAttandance = EndPointBase + "/staff/load";
            public const string TakeAttandance = EndPointBase + "/staff/takeAttandance";
        }

        public static class WalletEndPoint
        {
            public const string EndPointBase = ApiEndpoint + "/wallet";
            public const string TopUpWallet = EndPointBase + "/topup";
        }

        public static class NotificationEndPoint
        {
            public const string EndPointBase = ApiEndpoint + "/notification";
            public const string GetNotifications = EndPointBase + "/user";
            public const string GetStaffNotifications = EndPointBase + "/staff";
            public const string UpdateNotification = EndPointBase + "/update";
            public const string DeleteNotification = EndPointBase + "/{id}/delete";
            public const string DirectPushNotification = EndPointBase + "/direct/pushNotification";
        }

        public static class QuizEndPoint
        {
            public const string EndPointBase = ApiEndpoint + "/exams/quizzes";
            public const string GetCurrentStudentQuizDone = ApiEndpoint + "/exams/quiz/current/done";
            public const string GradeQuizMC = ApiEndpoint + "/exam/quiz/multipleChoice/grade";
            public const string GradeQuizOffLine = ApiEndpoint + "/exam/quiz/offLine/grade";
            public const string GradeQuizFC = ApiEndpoint + "/exam/quiz/flashCard/grade";
            public const string GetQuizOverallByCourseId = ApiEndpoint + "/exams/quizzes/course";
            public const string GetExamOffClassByClassId = ApiEndpoint + "/exams/class";
            public const string GetExamOffCurrentStudentByTime = ApiEndpoint + "/exams/student/byTime";
            public const string GetQuizOffExamByExamId = ApiEndpoint + "/exam/quiz";
            public const string GetQuizForStaff = ApiEndpoint + "/{id}/staff/get";
            public const string UpdateQuizForStaff = ApiEndpoint + "/{questionpackageId}/update";
        }
    }
}
