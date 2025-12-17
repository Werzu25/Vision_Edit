namespace Tools;

public class Validation
{
    public static bool ValidString(string input, int minLength = 0, int maxLength = 0)
    {
        if (input == null)
        {
            return false;
        }

        if (minLength > 0 && input.Length < minLength)
        {
            return false;
        }

        if (maxLength > 0 && input.Length > maxLength)
        {
            return false;
        }

        return true;
    }

    public static bool ValidDate(DateTime input)
    {
        if (input > DateTime.Now)
        {
            return false;
        }

        return true;
    }

    public static bool ValidPassword(string input, int minLength = 0, int maxLength = 0, int minUppercase = 0,
        int mindLowercase = 0, int minCountDigits = 0, string specialChars = "", int minSpecialChars = 0)
    {
        if (input == null)
        {
            return false;
        }

        if (minLength > 0 && input.Length < minLength)
        {
            return false;
        }

        if (maxLength > 0 && input.Length > maxLength)
        {
            return false;
        }

        if (input.Any(char.IsUpper) && input.Any(char.IsLower))
        {
            int countU = input.Count(c => char.IsUpper(c));
            int countL = input.Count(c => char.IsLower(c));
            if (minUppercase > 0 && countU < minUppercase)
            {
                return false;
            }

            if (mindLowercase > 0 && countL < mindLowercase)
            {
                return false;
            }
        }

        if (minCountDigits > 0 && input.Count(char.IsDigit) < minCountDigits)
        {
            return false;
        }

        if (input.Contains(specialChars))
        {
            if (minSpecialChars > 0 && input.Count(c => specialChars.Contains(c)) < minSpecialChars)
            {
                return false;
            }
        }

        return true;
    }

    public static bool ValidUsername(string input, List<String> existingUsernames)
    {
        if (existingUsernames.Contains(input))
        {
            return false;
        }

        return true;
    }
}