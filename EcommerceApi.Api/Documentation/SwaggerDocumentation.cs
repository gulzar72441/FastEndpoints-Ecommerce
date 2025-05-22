using System.Runtime.CompilerServices;

namespace EcommerceApi.Api.Documentation
{
    /// <summary>
    /// This class contains documentation for the Swagger UI.
    /// </summary>
    public static class SwaggerDocumentation
    {
        /// <summary>
        /// Provides examples of common API responses.
        /// </summary>
        public static class Examples
        {
            /// <summary>
            /// Example of a successful response.
            /// </summary>
            public const string Success = "The request was successful.";

            /// <summary>
            /// Example of a bad request response.
            /// </summary>
            public const string BadRequest = "The request was invalid or cannot be otherwise served.";

            /// <summary>
            /// Example of an unauthorized response.
            /// </summary>
            public const string Unauthorized = "Authentication credentials were missing or incorrect.";

            /// <summary>
            /// Example of a forbidden response.
            /// </summary>
            public const string Forbidden = "The request is understood, but it has been refused or access is not allowed.";

            /// <summary>
            /// Example of a not found response.
            /// </summary>
            public const string NotFound = "The requested resource could not be found.";

            /// <summary>
            /// Example of a server error response.
            /// </summary>
            public const string ServerError = "Something went wrong on the server.";
        }

        /// <summary>
        /// Provides documentation for common API parameters.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Documentation for the ID parameter.
            /// </summary>
            public const string Id = "The unique identifier for the resource.";

            /// <summary>
            /// Documentation for pagination parameters.
            /// </summary>
            public const string Pagination = "Page number and size for pagination.";

            /// <summary>
            /// Documentation for sorting parameters.
            /// </summary>
            public const string Sorting = "Field and direction for sorting results.";

            /// <summary>
            /// Documentation for filtering parameters.
            /// </summary>
            public const string Filtering = "Criteria for filtering results.";
        }

        /// <summary>
        /// Provides documentation for authentication.
        /// </summary>
        public static class Authentication
        {
            /// <summary>
            /// Documentation for JWT authentication.
            /// </summary>
            public const string Jwt = "JWT token obtained from the login endpoint.";

            /// <summary>
            /// Documentation for admin authentication.
            /// </summary>
            public const string Admin = "Requires admin role.";

            /// <summary>
            /// Documentation for customer authentication.
            /// </summary>
            public const string Customer = "Requires customer role.";
        }
    }
}
