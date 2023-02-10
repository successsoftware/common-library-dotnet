using FluentValidation.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SSS.AspNetCore.Extensions.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace SSS.AspNetCore.Extensions.Handlers
{
    /// <summary>
    /// Handle global exception. There are application exception will be handle if the message matches:
    /// concurrency
    /// </summary>
    public static class GlobalExceptionHandling
    {
        public static NamingStrategy NamingPolicy { get; set; } = new CamelCaseNamingStrategy();

        private const string CONCURRENCY_ERROR = "concurrency";
        private const string INVALID_ARGUMENT = "invalid_argument";
        private const string ARGUMENT_ERROR_MESSAGGE = "One or more validation errors occurred.";
        public const string CONCURRENCY_ERROR_MESSAGE = "An concurrent error occur when execute this request. Please retry again";
        public const string INTERNALE_ERROR_MESSAGE = "An error occurred. Please try again later.";

        internal static ProblemDetails GetProblemDetails(this ArgumentNullException argumentNullException)
        {
            if (argumentNullException is ArgumentException argument)
                return argument.GetProblemDetails();

            return new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                TraceId = "",
                ErrorCode = INVALID_ARGUMENT,
                ErrorMessage = argumentNullException?.Message
            };
        }

        internal static ProblemDetails GetProblemDetails(this ArgumentException argumentException)
        {
            var error = new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                TraceId = "",
                ErrorCode = INVALID_ARGUMENT,
                ErrorMessage = argumentException?.Message
            };

            if (string.IsNullOrEmpty(argumentException.ParamName))
            {
                if (!string.IsNullOrEmpty(argumentException.Message))
                {
                    error.ErrorMessage = argumentException.Message;
                }
            }
            else
            {
                error.ErrorDetails.Add(argumentException.ParamName, new[] { ExcludingParameterFor(argumentException.Message) });
                error.ErrorMessage = ARGUMENT_ERROR_MESSAGGE;
            }

            return error;
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/dotnet/api/system.argumentexception.message?view=netcore-3.1
        /// The message string appended with the name of the invalid parameter. 
        /// this method will find out the origin message
        /// </summary>
        private static string ExcludingParameterFor(string argumentExceptionMessage)
        {
            if (string.IsNullOrEmpty(argumentExceptionMessage)) return string.Empty;
            if (argumentExceptionMessage.IndexOf(' ') > 0)
            {
                return argumentExceptionMessage[..argumentExceptionMessage.IndexOf(' ')];
            }
            return argumentExceptionMessage;
        }

        internal static ProblemDetails GetProblemDetails(this BadRequestException badRequestException)
        {
            return new ProblemDetails()
            {
                Status = (int)HttpStatusCode.BadRequest,
                ErrorCode = ARGUMENT_ERROR_MESSAGGE,
                ErrorMessage = badRequestException.Message
            };
        }

        internal static ProblemDetails GetProblemDetails(this NotFoundException notFoundException)
        {
            return new ProblemDetails()
            {
                Status = (int)HttpStatusCode.NotFound,
                ErrorCode = INVALID_ARGUMENT,
                ErrorMessage = notFoundException.Message
            };
        }

        internal static ProblemDetails GetProblemDetails(this ValidationException validationException)
        {
            var error = new ProblemDetails()
            {
                Status = (int)HttpStatusCode.BadRequest,
                TraceId = "",
                ErrorCode = INVALID_ARGUMENT,
                ErrorMessage = validationException.Message
            };

            if (TryParseProblemDetail(validationException.Message, out var problem))
            {
                if (problem?.Status == 0)
                {
                    problem.Status = (int)HttpStatusCode.BadRequest;
                }
                return problem;
            }

            if (validationException.ValidationResult == null)
            {
                return error;
            }

            foreach (var member in validationException.ValidationResult?.MemberNames)
            {
                error.ErrorDetails.Add(member, new[] { validationException.ValidationResult.ErrorMessage });
                error.ErrorMessage = ARGUMENT_ERROR_MESSAGGE;
            }

            return error;
        }

        internal static ProblemDetails GetProblemDetails(this FluentValidation.ValidationException validationException)
        {
            var error = new ProblemDetails()
            {
                Status = (int)HttpStatusCode.BadRequest,
                TraceId = "",
                ErrorCode = INVALID_ARGUMENT,
                ErrorMessage = ARGUMENT_ERROR_MESSAGGE
            };

            if (TryParseProblemDetail(validationException.Errors, out var problem))
            {
                problem.Status = (int)HttpStatusCode.BadRequest;

                return problem;
            }

            return error;
        }

        internal static ProblemDetails GetProblemDetails(this AggregateException aggregateException)
        {
            var error = new ProblemDetails()
            {
                Status = (int)HttpStatusCode.BadRequest,
                TraceId = "",
                ErrorCode = INVALID_ARGUMENT,
                ErrorMessage = aggregateException.Message
            };

            var validationExceptions = aggregateException.InnerExceptions?
                                            .Where(ex => ex is ValidationException)
                                            .OfType<ValidationException>();

            if (validationExceptions == null || !validationExceptions.Any())
            {
                return error;
            }

            foreach (var exception in validationExceptions)
            {
                var (members, errorMessage) = exception.ValidationResult.GetError();
                foreach (var member in members)
                {
                    var errorList = new List<string>
                    {
                        errorMessage
                    };

                    var isExist = error.ErrorDetails.TryGetValue(member, out string[] errorMessages);
                    if (isExist)
                    {
                        error.ErrorDetails.Remove(member);
                        foreach (var msg in errorMessages)
                        {
                            errorList.Add(msg);
                        }
                    }

                    error.ErrorDetails.Add(member, errorList.Distinct().ToArray());
                }
            }
            if (error.ErrorMessage?.Any() == true)
            {
                error.ErrorMessage = ARGUMENT_ERROR_MESSAGGE;
            }

            return error;
        }

        internal static ProblemDetails GetProblemDetails(this ApplicationException validationException)
        {
            var error = new ProblemDetails()
            {
                Status = (int)HttpStatusCode.BadRequest,
                TraceId = "",
                ErrorCode = INVALID_ARGUMENT,
                ErrorMessage = validationException.Message
            };

            if (validationException.Message == CONCURRENCY_ERROR)
            {
                error.ErrorCode = nameof(CONCURRENCY_ERROR);
                error.ErrorMessage = CONCURRENCY_ERROR_MESSAGE;
                error.Status = (int)HttpStatusCode.Conflict;
            }

            return error;
        }

        internal static ProblemDetails GetProblemDetails(this Refit.ApiException apiException)
        {
            if (apiException.StatusCode == HttpStatusCode.BadRequest)
            {
                try
                {
                    ProblemDetails problem = null;
                    if (apiException is Refit.ValidationApiException exception)
                    {
                        var validationProblem = exception.Content;
                        if (validationProblem == null)
                        {
                            throw new ArgumentException("Cannot handle problem detail from ApiException");
                        }

                        problem = new ProblemDetails()
                        {
                            Status = (int)HttpStatusCode.BadRequest,
                            ErrorCode = INVALID_ARGUMENT,
                            ErrorMessage = validationProblem.Detail
                        };

                        if (validationProblem.Errors != null)
                            problem.ErrorDetails = validationProblem.Errors;
                    }
                    else
                    {
                        problem = JsonConvert.DeserializeObject<ProblemDetails>(apiException.Content);
                    }
                    return problem;
                }
                catch
                {
                    Trace.TraceError($"Error when trying to convert ValidationProblemDetail from upstream. Content:{apiException.Content}");
                }
            }

            return apiException.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new ProblemDetails() { Status = (int)HttpStatusCode.Unauthorized, ErrorCode = nameof(HttpStatusCode.Unauthorized), ErrorMessage = apiException.Content },
                HttpStatusCode.Forbidden => new ProblemDetails() { Status = (int)HttpStatusCode.Forbidden, ErrorCode = nameof(HttpStatusCode.Forbidden), ErrorMessage = apiException.Content },
                HttpStatusCode.NotFound => new ProblemDetails() { Status = (int)HttpStatusCode.NotFound, ErrorCode = nameof(HttpStatusCode.NotFound), ErrorMessage = $"Resource not found" },
                HttpStatusCode.TooManyRequests => new ProblemDetails() { Status = (int)HttpStatusCode.TooManyRequests, ErrorCode = nameof(HttpStatusCode.TooManyRequests), ErrorMessage = $"You're reached the maximum request threshold. Please retry later." },
                _ => new ProblemDetails() { ErrorCode = apiException.StatusCode.ToString(), ErrorMessage = apiException.Content }
            };
        }

        public static string ToJSON(this ProblemDetails problemDetails, bool enableIndented = false)
        {
            if (problemDetails == null) return string.Empty;

            var selfType = problemDetails.GetType();
            Debug.WriteLine($"Self type: {selfType}");

            var problemAsTransformed = TransformToNamingPolicy(problemDetails, NamingPolicy);

            if (enableIndented)
            {
                return JsonConvert.SerializeObject(problemAsTransformed, selfType, Formatting.Indented, null);
            }

            return JsonConvert.SerializeObject(problemAsTransformed, selfType, Formatting.None, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = NamingPolicy
                }
            });
        }

        private static bool TryParseProblemDetail(string message, out ProblemDetails problem)
        {
            try
            {
                problem = JsonConvert.DeserializeObject<ProblemDetails>(message);

                if (!string.IsNullOrEmpty(problem.ErrorCode))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                problem = null;

                return false;
            }
        }

        private static bool TryParseProblemDetail(IEnumerable<ValidationFailure> errors, out ProblemDetails problem)
        {
            try
            {
                var errorsDetail = new Dictionary<string, string[]>();

                foreach (var error in errors)
                {
                    errorsDetail.TryAdd(error?.PropertyName, new string[] { error?.ErrorMessage });
                }

                problem = new ProblemDetails(errorsDetail);

                return true;
            }
            catch
            {
                problem = null;

                return false;
            }
        }

        private static ProblemDetails TransformToNamingPolicy(ProblemDetails problemDetails, NamingStrategy namingPolicy)
        {
            if (problemDetails == null)
            {
                Trace.TraceError($"Figure out a ProblemDetails which cannot do transform to NamingPolicy");
                return problemDetails;
            }

            if (problemDetails.ErrorDetails == null || problemDetails.ErrorDetails.Count == 0)
            {
                Debug.WriteLine($"No Error Detail to transform");
                return problemDetails;
            }

            var errors = new Dictionary<string, string[]>();

            foreach (var item in problemDetails.ErrorDetails)
            {
                errors.Add(namingPolicy.GetPropertyName(item.Key, false), item.Value);
            }

            problemDetails.ErrorDetails = errors;

            return problemDetails;
        }

        private static (List<string> members, string errorMessage) GetError(this System.ComponentModel.DataAnnotations.ValidationResult validationResult)
        {
            var members = new List<string>();
            if (validationResult.MemberNames == null || !validationResult.MemberNames.Any())
            {
                return (members, string.Empty);
            }

            foreach (var item in validationResult.MemberNames)
            {
                members.Add(item);
            }

            return (members, validationResult.ErrorMessage);
        }
    }
}