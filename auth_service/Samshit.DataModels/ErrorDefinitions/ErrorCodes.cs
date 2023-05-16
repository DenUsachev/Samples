namespace Samshit.DataModels.ErrorDefinitions
{
    /// <summary>
    /// Содержит определения кодов ошибок, известные системе
    /// </summary>
    public static class ErrorCodes
    {
        public static string GenericError = "ERR_GENERIC";
        public static string MicroserviceNegotiationError = "ERR_NEGOTIATE_INFRASTRUCTURE";
        public static string MicroserviceTimeoutError = "ERR_NEGOTIATE_INFRASTRUCTURE";
        public static string OperationError = "ERR_OPERATION_INVALID";
        public static string AccountDisabledError = "ERR_ACCOUNT_DISABLED";
        public static string RecoveryTokenExpiredError = "ERR_RECOVERY_TOKEN_EXPIRED_ERROR";

        public static string LoginError = "ERR_LOGIN_INVALID";
        public static string LogoutError = "ERR_LOGOUT_INVALID";
        public static string ModelError = "ERR_MODEL_INVALID";
        public static string EntityNotFoundError = "ERR_ENTITY_NOT_FOUND"; 
        public static string DbWriteError = "ERR_DB_WRITE"; 
        public static string DbAlterError = "ERR_DB_MODIFY"; 
        public static string ModelAddError = "ERR_MODEL_ADD"; 
        public static string ModelGetError = "ERR_MODEL_GET"; 
        public static string ModelEditError = "ERR_MODEL_MODIFY"; 
        public static string ModelExistError = "ERR_MODEL_EXIST"; 
    }
}