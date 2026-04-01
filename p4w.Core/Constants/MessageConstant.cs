namespace p4w.Core.Constants;

public static class MessageConstant
{
    public static class CommonMessage
    {
        public const string UNAUTHORIZED = "Common_401";
        public const string ACCESS_DENIED = "Common_403";
        public const string NOT_FOUND = "Common_404";
        public const string INTERNAL_SERVER_ERROR = "Common_500";
        public const string MISSING_PARAM = "Common_501";
    }

    public static class AuthMessage
    {
        public const string INVALID_USERNAME_OR_PASSWORD = "Auth_000";
        public const string USER_LOCKED = "Auth_001";
        public const string LOGIN_SUCCESS = "Auth_002";
        public const string LOGOUT_SUCCESS = "Auth_003";
        public const string PROFILE_UPDATED_SUCCESS = "Auth_004";
        public const string EMAIL_ALREADY_IN_USE = "Auth_005";
        public const string INVALID_TOKEN = "Auth_006";
        public const string INVALID_REFRESH_TOKEN = "Auth_007";
        public const string REFRESH_TOKEN_MISMATCH = "Auth_008";
        public const string REFRESH_TOKEN_EXPIRED = "Auth_009";
        public const string GOOGLE_TOKEN_EXCHANGE_FAILED = "Auth_010";
        public const string GOOGLE_MISSING_ID_TOKEN = "Auth_011";
        public const string GOOGLE_MISSING_CODE = "Auth_012";
        public const string LOGIN_FAILED = "Auth_013";
    }

    public static class UserMessage
    {
        public const string USER_NOT_FOUND = "User_000";
        public const string USER_PROFILE_RETRIEVED_SUCCESS = "User_001";
        public const string USER_RECENT_LOCATION_EMPTY = "User_002";
        public const string USER_RECENT_LOCATION_RETRIEVED_SUCCESS = "User_003";
        public const string USERS_RETRIEVED_SUCCESS = "User_004";
        public const string USER_RETRIEVED_SUCCESS = "User_005";
        public const string USER_CREATED_SUCCESS = "User_006";
        public const string USER_UPDATED_SUCCESS = "User_007";
        public const string USER_LOCKED_SUCCESS = "User_008";
        public const string USER_UNLOCKED_SUCCESS = "User_009";
    }

    public static class LocationMessage
    {
        public const string LOCATION_NOT_FOUND = "Location_000";
        public const string LOCATIONS_RETRIEVED_SUCCESS = "Location_001";
        public const string LOCATION_DETAIL_RETRIEVED_SUCCESS = "Location_002";
        public const string LOCATION_REVIEWS_RETRIEVED_SUCCESS = "Location_003";
        public const string LOCATION_CREATED_PENDING_APPROVAL = "Location_004";
        public const string LOCATION_UPDATED_PENDING_APPROVAL = "Location_005";
        public const string LOCATION_UPDATE_ACCESS_DENIED = "Location_006";
        public const string INACTIVE_LOCATION_CANNOT_BE_UPDATED = "Location_007";
        public const string LOCATION_NAME_REQUIRED = "Location_008";
        public const string ADDRESS_REQUIRED = "Location_009";
        public const string TYPE_REQUIRED = "Location_010";
        public const string STATUS_REQUIRED = "Location_011";
        public const string LOCATION_STATUS_INVALID = "Location_012";
        public const string TIME_FORMAT_INVALID = "Location_013";
        public const string ADMIN_LOCATIONS_RETRIEVED_SUCCESS = "Location_014";
        public const string ADMIN_LOCATION_DETAIL_RETRIEVED_SUCCESS = "Location_015";
        public const string ADMIN_LOCATION_CREATED_SUCCESS = "Location_016";
        public const string ADMIN_LOCATION_UPDATED_SUCCESS = "Location_017";
        public const string ADMIN_LOCATION_HIDDEN_SUCCESS = "Location_018";
    }

    public static class ReviewMessage
    {
        public const string REVIEW_NOT_FOUND = "Review_000";
        public const string REVIEW_CREATED_SUCCESS = "Review_001";
        public const string RATING_INVALID = "Review_002";
        public const string REVIEW_CONTENT_REQUIRED = "Review_003";
        public const string REVIEW_MAX_IMAGES = "Review_004";
        public const string REVIEW_COMMENTS_RETRIEVED_SUCCESS = "Review_005";
        public const string REVIEW_STATUS_INVALID = "Review_006";
        public const string ADMIN_REVIEWS_RETRIEVED_SUCCESS = "Review_007";
        public const string ADMIN_REVIEW_DETAIL_RETRIEVED_SUCCESS = "Review_008";
        public const string ADMIN_REVIEW_STATUS_UPDATED_SUCCESS = "Review_009";
        public const string ADMIN_REVIEW_HIDDEN_SUCCESS = "Review_010";
    }

    public static class CommentMessage
    {
        public const string COMMENT_CREATED_SUCCESS = "Comment_000";
        public const string COMMENT_CONTENT_REQUIRED = "Comment_001";
        public const string PARENT_COMMENT_INVALID = "Comment_002";
        public const string ADMIN_COMMENTS_RETRIEVED_SUCCESS = "Comment_003";
        public const string ADMIN_COMMENT_DETAIL_RETRIEVED_SUCCESS = "Comment_004";
        public const string ADMIN_COMMENT_HIDDEN_SUCCESS = "Comment_005";
        public const string COMMENT_NOT_FOUND = "Comment_006";
    }

    public static class ReportMessage
    {
        public const string REASON_REQUIRED = "Report_000";
        public const string TARGET_TYPE_INVALID = "Report_001";
        public const string TARGET_ID_REQUIRED = "Report_002";
        public const string TARGET_NOT_FOUND = "Report_003";
        public const string REPORT_NOT_FOUND = "Report_004";
        public const string STATUS_INVALID = "Report_005";
        public const string REPORT_CREATED_SUCCESS = "Report_006";
        public const string REPORTS_RETRIEVED_SUCCESS = "Report_007";
        public const string REPORT_DETAIL_RETRIEVED_SUCCESS = "Report_008";
        public const string REPORT_STATUS_UPDATED_SUCCESS = "Report_009";
    }

    public static class UploadMessage
    {
        public const string NO_FILE_PROVIDED = "Upload_000";
        public const string FILE_UPLOADED_SUCCESS = "Upload_001";
    }

    public static class DashboardMessage
    {
        public const string ADMIN_DASHBOARD_RETRIEVED_SUCCESS = "Dashboard_000";
    }
}
