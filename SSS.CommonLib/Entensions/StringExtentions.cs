using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SSS.CommonLib.Entensions
{
    public static class StringExtentions
    {
        private const string LowerCase = "abcdefghijklmnopqursuvwxyz";
        private const string UpperCaes = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Numbers = "123456789";
        private const string Specials = @"!@£$%^&*()#€";
        private const string EmailPattern = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*"
+ "@"
+ @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";

        public static string AddSpacesToString(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            var newText = new StringBuilder(text.Length * 2);

            newText.Append(text[0]);

            for (var i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                    newText.Append(' ');

                newText.Append(text[i]);
            }

            return newText.ToString();
        }

        public static string RemoveWhiteSpace(this string input)
        {
            return new string((from c in input.ToCharArray()
                               where !char.IsWhiteSpace(c)
                               select c).ToArray());
        }

        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input.First().ToString().ToUpper(), input.AsSpan(1))
            };

        public static string FirstCharToLower(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input.First().ToString().ToLower(), input.AsSpan(1))
            };

        public static bool IsDigitsOnly(this string str)
        {
            return str.All(c => c is >= '0' and <= '9');
        }

        public static string GeneratePassword(bool useLowercase, bool useUppercase, bool useNumbers, bool useSpecial, int passwordSize)
        {
            var password = new char[passwordSize];
            var charSet = string.Empty;
            Random random = new();
            int counter;

            // Build up the character set to choose from
            if (useLowercase) charSet += LowerCase;

            if (useUppercase) charSet += UpperCaes;

            if (useNumbers) charSet += Numbers;

            if (useSpecial) charSet += Specials;

            for (counter = 0; counter < passwordSize; counter++)
            {
                password[counter] = charSet[random.Next(charSet.Length - 1)];
            }

            return string.Join("", password);
        }

        public static bool IsEmail(this string value)
        {
            var regex = new Regex(EmailPattern);

            var match = regex.Match(value);

            return match.Success;
        }

        public static string ConvertCamelCaseToSenteceCase(this string value)
        {
            value = value.FirstCharToUpper();
            const string rex = "(?<!^)([A-Z])";
            var rgx = new Regex(rex);
            return rgx.Replace(value, " $1");
        }
    }
}
