using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SSS.AspNetCore.Extensions.Handlers
{
    public static class TransformModelStateExtensions
    {
        private const string INVALID_ARGUMENT = "invalid_argument";

        internal static IActionResult TransformModelState(ModelStateDictionary errorModelState, NamingStrategy namingStrategy)
        {
            var problemDetails = new ProblemDetails()
            {
                TraceId = Activity.Current?.RootId ?? Activity.Current?.TraceId.ToString() ?? string.Empty,
                ErrorCode = INVALID_ARGUMENT,
                ErrorMessage = "One or more validation errors occurred."
            };

            foreach (var member in errorModelState)
            {
                var key = namingStrategy != null ?
                            namingStrategy.GetPropertyName(member.Key, false) : member.Key;

                problemDetails.ErrorDetails.Add(key, new[] { member.Value?.Errors?.FirstOrDefault()?.ErrorMessage });
            }

            problemDetails.ErrorCode = problemDetails.ErrorCode.ToLower();
            return new BadRequestObjectResult(problemDetails);
        }

        private enum SeparatedCaseState
        {
            Start,
            Lower,
            Upper,
            NewWord,
            DotChar
        }

        public static string ToSnakeCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            const char separator = '_';
            const char dotChar = '.';

            StringBuilder sb = new StringBuilder();
            SeparatedCaseState state = SeparatedCaseState.Start;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ')
                {
                    if (state != SeparatedCaseState.Start)
                    {
                        state = SeparatedCaseState.NewWord;
                    }
                }
                else if (s[i] == dotChar)
                {
                    sb.Append(dotChar);
                    state = SeparatedCaseState.DotChar;
                }
                else if (char.IsUpper(s[i]))
                {
                    switch (state)
                    {
                        case SeparatedCaseState.Upper:
                            bool hasNext = (i + 1 < s.Length);
                            if (i > 0 && hasNext)
                            {
                                char nextChar = s[i + 1];
                                if (!char.IsUpper(nextChar) && nextChar != separator)
                                {
                                    sb.Append(separator);
                                }
                            }
                            break;
                        case SeparatedCaseState.Lower:
                        case SeparatedCaseState.NewWord:
                            sb.Append(separator);
                            break;
                    }

                    char c;

                    c = char.ToLowerInvariant(s[i]);

                    sb.Append(c);

                    state = SeparatedCaseState.Upper;
                }
                else if (s[i] == separator)
                {
                    sb.Append(separator);
                    state = SeparatedCaseState.Start;
                }
                else
                {
                    if (state == SeparatedCaseState.NewWord)
                    {
                        sb.Append(separator);
                    }

                    sb.Append(s[i]);
                    state = SeparatedCaseState.Lower;
                }
            }

            return sb.ToString();
        }
    }
}
