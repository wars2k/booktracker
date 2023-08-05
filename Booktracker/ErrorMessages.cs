namespace bookTrackerApi {

    public static class ErrorMessages {
        public static ErrorMessage missing_request_body = new ErrorMessage {
            Code = "missing_request_body",
            Message = "Request failed due to missing request body.",
            Details = "Proper request body format can be found in the Swagger API docs."
        };

        public static ErrorMessage invalid_request_body = new ErrorMessage {
            Code = "invalid_request_body",
            Message = "Request failed due to incorrectly formatted request body.",
            Details = "Proper request body formatting can be found in the Swagger API docs."
        };

        public static ErrorMessage missing_parameter = new ErrorMessage {
            Code = "missing_parameter",
            Message = "Request failed due to missing paramter.",
            Details = "Required request paramters can be found in the Swagger API docs."
        };

        public static ErrorMessage invalid_paramter = new ErrorMessage {
            Code = "invalid_parameter",
            Message = "Request failed due to invalid value for a required paramter.",
            Details = "For paramters that require specific values, check the Swagger API docs for the accepted values."
        };

        public static ErrorMessage invalid_sessionKey = new ErrorMessage {
            Code = "invalid_sessionKey",
            Message = "Request failed due to invalid or missing session key.",
            Details = "A valid session key is required for most requests. Session keys are created with the '/api/login' endpoint."
        };

        public static ErrorMessage invalid_privileges = new ErrorMessage {
            Code = "invalid_privileges",
            Message = "Request failed due to invalid privileges.",
            Details = "Some requests (updating books, deleting books, adding users, etc.) require admin privileges."
        };

    }

    public class ErrorMessage {

            public string? Code { get; set; }
            public string? Message { get; set; }
            public string? Details { get; set; }
            
    }

}