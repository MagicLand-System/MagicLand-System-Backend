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

        public static class DeveloperEndpoint
        {
            public const string TakeFullAttendanceAndEvaluate = ApiEndpoint + "/developer/attendanceAndEvaluate";
            public const string GetStudentAuthenAndExam = ApiEndpoint + "/developer/authenAndExam";

        }
        public static class UserEndpoint
        {
            public const string RootEndpoint = ApiEndpoint + "/users";
            public const string AddUser = ApiEndpoint + "/users/add";
            public const string GetUserById = RootEndpoint + "/{id}";
            public const string CheckExist = RootEndpoint + "checkExist";
            public const string GetCurrentUser = RootEndpoint + "/getcurrentuser";
            public const string Register = RootEndpoint + "/register";
            public const string CheckoutClass = RootEndpoint + "/checkout";
            public const string CheckOutClassByVnpay = RootEndpoint + "/vnpay/checkout";
            public const string GetLecturer = RootEndpoint + "/getLecturer";
            public const string UpdateUser = RootEndpoint + "/update";
            public const string GetByAdmin = RootEndpoint + "/getByAdmin";
            public const string GetByPhone = RootEndpoint + "/getByPhone";
            public const string GetStudent = RootEndpoint + "/getStudent";
            public const string GetFromName = RootEndpoint + "/getFromName";
            public const string GetStudentInfor = RootEndpoint + "/getStudentInfor";
            public const string GetClassOfStudent = RootEndpoint + "/getClassOfStudent";
            public const string GetScheduleOfStudent = RootEndpoint + "/getScheduleOfStudent";
            public const string GetSessionOfStudent = RootEndpoint + "/getSessionOfStudent";
            public const string GetListSessionOfStudent = RootEndpoint + "/getListSessionOfStudent";
        }
        public static class AuthenticationEndpoint
        {
            public const string Authentication = ApiEndpoint + "/auth";
            public const string Login = Authentication + "/login";
            public const string RefreshToken = ApiEndpoint + "/auth/refreshtoken";

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
            public const string GetFromClassCode = GetAll + "/classCode";
            public const string InsertClassesV2 =  GetAll + "/insertClassesV2";
            public const string InsertClassesV3 = GetAll + "/insertClasses/save";
            public const string RoomForUpdateClass = GetAll + "/getRoomForUpdate";
            public const string LecturerForUpdateClass = GetAll + "/getLecturerForUpdate";
            public const string LecturerForUpdateSession = GetAll + "/getLecturerForUpdateSession";
            public const string RoomForUpdateSession = GetAll + "/getRoomForUpdateSession";
            public const string SetNotCanMakeUp = GetAll + "/{scheduleId}/{studentId}/setStatusNotCanMakeUp";
            public const string GetListCanNotMakeUp = GetAll + "/getListCanNotMakeUp";
            public const string SaveCourse = GetAll + "/{classId}/{studentId}/SaveCouse";
            public const string GetListSavedCourse = GetAll + "/getListSavedCourse";
            public const string GetClassToRegister = GetAll + "/getClassToRegister";
        }
        public static class PromotionEnpoint
        {
            public const string GetAll = ApiEndpoint + "/promotion";
            public const string GetCurrent = GetAll + "/currentUser";
        }

        public static class CartEndpoint
        {
            public const string RootEndpoint = ApiEndpoint + "/cart";
            public const string ModifyCart = RootEndpoint + "/modify";
            public const string AddCourseFavoriteList = RootEndpoint + "/favorite/add";
            public const string GetCart = RootEndpoint + "/view";
            public const string GetFavorite = RootEndpoint + "/favorite/view";
            public const string DeleteCartItem = RootEndpoint + "/item/delete";
            public const string CheckOutCartItem = RootEndpoint + "/item/checkout";
            public const string GetAll = RootEndpoint + "/items/all";
            public const string CheckOutCartItemByVnpay = RootEndpoint + "/vnpay/item/checkout";
        }
        public static class CourseEndpoint
        {
            public const string GetAll = ApiEndpoint + "/courses";
            public const string GetAllValid = ApiEndpoint + "/courses/validRegister";
            public const string GetUserRelatedCourse = ApiEndpoint + "/courses/related";
            public const string GetCurrentStudentCourses = ApiEndpoint + "/courses/currentStudent";
            public const string SearchCourse = GetAll + "/search";
            public const string CourseById = GetAll + "/{id}";
            public const string FilterCourse = GetAll + "/filter";
            public const string GetCourseCategory = GetAll + "/categories";
            public const string AddCourse = GetAll + "/add";
            public const string GetCourseByStaff = GetAll + "/staff/get";
            public const string GetCoursePrice = GetAll + "/getCoursePrice";
            public const string AddPrice = GetAll + "/addPrice";
            public const string GetCourseStaff = GetAll + "/staff/getCourse";
            public const string GetCourseClassStaff = GetAll + "/staff/getClass";
            public const string UpdateCourse = GetAll + "/{id}/updateCourse";
            public const string GetAllStaff = ApiEndpoint + "/staff/courses";
            public const string GetAllStaffV2 = ApiEndpoint + "/staff/courses/search";
            public const string RegisterSavedCourse = ApiEndpoint + "/staff/courses/registerSaved";

        }
        public static class StudentEndpoint
        {
            public const string GetAll = ApiEndpoint + "/students";
            public const string FindValidDayReLearning = ApiEndpoint + "/student/dayOff/findClass";
            public const string GetStudentLearningProgress = ApiEndpoint + "/student/learning/progress";
            public const string StudentEndpointGet = GetAll + "/{id}";
            public const string StudentEnpointCreate = GetAll + "/add";
            public const string StudentEndpointGetClass = GetAll + "/getclass";
            public const string StudentGetSchedule = GetAll + "/getschedule";
            public const string GetStudentAccount = GetAll + "/getAccount";
            public const string GetStudentsOfCurrentUser = GetAll + "/currentuser";
            public const string UpdateStudent = GetAll + "/update";
            public const string DeleteStudent = GetAll + "/{id}/delete";
            public const string GetStudentCourseRegistered = GetAll + "/{id}/getcourses";
            public const string GetStatisticRegisterStudent= GetAll + "/register/statistic";
        }
        public static class RoomEndpoint
        {
            public const string GetAll = ApiEndpoint + "/rooms";
            public const string RoomById = GetAll + "/{id}";
            public const string RoomByAdmin = GetAll + "/admin/get";
            public const string RoomByAdminV2 = GetAll + "/v2/admin/get";
        }
        public static class SlotEndpoint
        {
            public const string GetAll = ApiEndpoint + "/slots";
            public const string SlotById = GetAll + "/{id}";
        }
        public static class WalletTransactionEndpoint
        {
            public const string GetAll = ApiEndpoint + "/walletTransactions";
            public const string TransactionById = GetAll + "/{id}";
            public const string PersonalWallet = GetAll + "/walletBalance";
            public const string GetBillTransactionById = GetAll + "/{id}/bill/status";
            public const string GetBillTransactionByTxnRefCode = GetAll + "/{txnRefCode}/bills/status";
            public const string GetRevenueTransactionByTime = GetAll + "/bills/revenue";
            public const string CheckoutByStaff = GetAll + "/staff/checkout";
        }
        public static class SyllabusEndpoint
        {
            public const string GetAll = ApiEndpoint + "/Syllabus";
            public const string CheckingSyllabusInfor = ApiEndpoint + "/Syllabus/infor/checking";
            public const string LoadByCourse = GetAll + "/getByCourse";
            public const string AddSyllabus = GetAll + "/insertSyllabus";
            public const string LoadSyllabus = GetAll + "/getById";
            public const string LoadSyllabuses = GetAll + "/getAll";
            public const string FilterSyllabus = GetAll + "/filter";
            public const string GeneralSyllabus = GetAll + "/general";
            public const string UpdateSyllabus = GetAll + "/{id}/update";
            public const string StaffSyl = GetAll + "/{id}/staff/get";
            public const string AvailableSyl = GetAll + "/available";
            public const string UpdateOverall = GetAll + "/{id}/updateOverall";
            public const string UpdateTopic = GetAll + "/{topicId}/updateTopic";
            public const string UpdateSession = GetAll + "/{descriptionId}/updateSession";
            public const string GenralInfromation = GetAll + "/staff/getGeneralInformation";
            public const string MaterialInfor = GetAll + "/staff/getMaterial";
            public const string ExamSyllabus = GetAll + "/staff/getExamSyllabus";
            public const string SessionSyllabus = GetAll + "/staff/getSessionSyllabus";
            public const string QuestionSyllabus = GetAll + "/staff/getQuestionSyllabus";
            public const string StaffFilterSyllabus = GetAll + "/staff/filter";

        }
        public static class LectureEndpoint
        {
            public const string GetAll = ApiEndpoint + "/lectures";
            public const string GetLecturerCareer = ApiEndpoint + "/lectures/career";
            public const string TakeStudentAttendance = GetAll + "/students/takeAttendance";
            public const string EvaluateStudent = GetAll + "/students/evaluate";
            public const string GetStudentEvaluates = GetAll + "/students/get/evaluates";
            public const string GetStudentQuizFullyInfor = GetAll + "/students/get/quiz/fullyInfor";
            public const string SettingQuizTime = GetAll + "/exam/quiz/setting";
            public const string GetStudentAttendance = GetAll + "/student/attendance";
            public const string GetStudentAttendanceOfAllClass = GetAll + "/student/classes/attendance";
            public const string GetCurrentClassesSchedule = GetAll + "/current/classes";
            public const string GetClassesAttendanceWithDate = GetAll + "/class/date/attendances";
            public const string GetLectureSchedule = GetAll + "/schedules";
        }

        public static class AttendanceEndpoint
        {
            public const string RootEndpoint = ApiEndpoint + "/attendance";
            public const string GetAttendanceOfClass = RootEndpoint + "/class/{id}";
            public const string GetAttendanceOfClasses = RootEndpoint + "/classes";
            public const string GetAttendanceOfStudent = RootEndpoint + "/student/{id}";
            public const string LoadAttandance = RootEndpoint + "/staff/load";
            public const string TakeAttandance = RootEndpoint + "/staff/takeAttandance";
            public const string TakeAttandanceV2 = RootEndpoint + "/staff/takeAttandance1";

        }

        public static class WalletEndpoint
        {
            public const string RootEndpoint = ApiEndpoint + "/wallet";
            public const string TopUpWallet = RootEndpoint + "/topup";
        }

        public static class NotificationEndpoint
        {
            public const string RootEndpoint = ApiEndpoint + "/notification";
            public const string GetNotifications = RootEndpoint + "/user";
            public const string GetStaffNotifications = RootEndpoint + "/staff";
            public const string UpdateNotification = RootEndpoint + "/update";
            public const string DeleteNotification = RootEndpoint + "/{id}/delete";
            public const string DirectPushNotification = RootEndpoint + "/direct/pushNotification";
        }

        public static class QuizEndpoint
        {
            public const string GetAll = ApiEndpoint + "/exams/quizzes";
            public const string GetCurrentStudentQuizDone = ApiEndpoint + "/exams/quiz/current/done";
            public const string GetCurrentStudentQuizWork = ApiEndpoint + "/exams/quiz/current/work";
            public const string GetFinalResult = ApiEndpoint + "/exams/quiz/finalResult";
            public const string GradeQuizMC = ApiEndpoint + "/exam/quiz/multipleChoice/grade";
            public const string GradeQuizOffLine = ApiEndpoint + "/exam/quiz/offLine/grade";
            public const string EvaluateQuizOnLine = ApiEndpoint + "/exam/quiz/onLine/evaluate";
            public const string GradeQuizFC = ApiEndpoint + "/exam/quiz/flashCard/grade";
            public const string GetFCQuestionPackage = ApiEndpoint + "/exam/quiz/flashCard/package";
            public const string GetQuizOverallByCourseId = ApiEndpoint + "/exams/quizzes/course";
            public const string GetExamOffClassByClassId = ApiEndpoint + "/exams/class";
            public const string GetStudentInforAndScore = ApiEndpoint + "/exams/class/students/score";
            public const string GetExamOffCurrentStudentByTime = ApiEndpoint + "/exams/student/byTime";
            public const string GetQuizOffExamByExamId = ApiEndpoint + "/exam/quiz";
            public const string GetQuizForStaff = ApiEndpoint + "/{id}/staff/get";
            public const string UpdateQuizForStaff = ApiEndpoint + "/{questionpackageId}/update";
        }
    }
}
