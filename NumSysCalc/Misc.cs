namespace NumSysCalc;
using System.Text;

public static class Misc
{
    public static string Repeat(this string str, in int count)
    {
        return string.Concat(Enumerable.Repeat(str, count));
    }

    public static string AddTrailingZeros(this string str)
    {
        if (!str.Contains('.') && !str.Contains(','))
        {
            str += ".0";
        }
        return str;
    }

    public static string RemoveTrailingZeros(this string str)
    {
        if (!str.Contains('.')) 
            throw new Exception("The string str parameter must contain trailing zeros in RemoveTrailingZeros method");
        int lastIndexOfComma = str.IndexOf('.');
        if (str.Substring(lastIndexOfComma + 1).Count(c => c == '0') == str.Substring(lastIndexOfComma + 1).Length)
            str = str.Substring(0, lastIndexOfComma);
        return str;
    }
    public static void AddTrailingZeros(this StringBuilder str)
    {
        if (!str.ToString().Contains('.') && !str.ToString().Contains(','))
        {
            str.Append(".0");
        }
    }

    public static StringBuilder Substring(this StringBuilder str, in int startIndex)
    {
        var substring = new StringBuilder(str.Length - startIndex);
        for (int i = startIndex; i <= str.Length - 1; i++)
        {
            substring.Append(str[i]);
        }
        
        return substring;
    }
    
    public static StringBuilder Substring(this StringBuilder str, in int startIndex, in int endIndex)
    {
        var substring = new StringBuilder(endIndex - startIndex + 1);
        for (int i = startIndex; i <= endIndex; i++)
        {
            substring.Append(str[i]);
        }
        
        return substring;
    }

    public static int IndexOfChar(this StringBuilder str, in char searchedChar)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == searchedChar) return i;
        }
        return -1;
    }

    public static string RemoveUselessZeros(this string str)
    {
        if (!str.Contains('.')  && !str.Contains(',')) return str;
        int indexOfComma = str.IndexOf('.');
        int numberOfZerosInBeginning = 0;
        int numberOfZerosInEnd = 0;
        
        for (int i = 0; i < indexOfComma; i++)
        {
            if (str[i] == '0')
            {
                numberOfZerosInBeginning++;
            }
            else break;
        }

        for (int i = str.Length - 1; i > indexOfComma; i--)
        {
            if (str[i] == '0')
            {
                numberOfZerosInEnd++;
            }
            else break;
        }

        return str.Substring(numberOfZerosInBeginning, str.Length - numberOfZerosInEnd);
    }
    
}