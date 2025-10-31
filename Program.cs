using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;


namespace NumSys3;

public static class Misc
{
    public static bool IsNegative(this string str)
    {
        if (str[0] == '-') return true;
        return false;
    }
    
    public static bool IsPositive(this string str)
    {
        if (str[0] != '-') return true;
        return false;
    }

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
        for (int i = startIndex; i <= substring.Length - 1; i++)
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
}
class NumSys
{
    public const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMN";
    private const string InputPattern = @"^(?!-+$)(?!.*-.*-)[0-9a-zA-MN\-,.]+$";

    private string _number;
    private int _numberBase;

    // Custom properties
    public string Number
    {
        get { return _number; }
        set
        {
            if (Regex.IsMatch(value, InputPattern)) _number = value; 
            else throw new ArgumentException("The number should contain only alphabet characters and one minus at the beginning");
        }
    }
    
    public int NumberBase
    {
        get { return _numberBase; }
        set
        {
            if (value >= 1 && value <= 50) _numberBase = value; 
            else throw new ArgumentException("The base should be lower than 50 and higher than 0");
        }
    }

    // Custom constructor
    public NumSys(string number, int numberBase)
    {
        if (Regex.IsMatch(number, InputPattern)) this._number = number;
        else throw new ArgumentException("The number should contain only alphabet characters and one minus at the beginning");
        if (numberBase >= 1 && numberBase <= 50) this.NumberBase = numberBase;
        else throw new ArgumentException("The base should be lower than 50 and higher than 0");
    }

    public override string ToString()
    {
        return this.Number + "^^" + this.NumberBase.ToString();
    }

    public static NumSys Parse(string number, int numberBase)
    {
        return new NumSys(number, numberBase);
    }

    public static bool ValidateNumber(string number)
    {
        if (Regex.IsMatch(number, InputPattern)) return true;
        else return false;
    }

    public static bool ValidateBase(int numberBase)
    {
        if (numberBase >= 1 && numberBase <= 50) return true;
        else return false;
    }
    public NumSys FromAnyBaseToDecimal()
    {
        string input = this.Number;
        int origBase = this.NumberBase;
        // No comments, too obvious
        if (origBase == 10)
        {
            return this;
        } 
    
        // Working with unary number system
        if (origBase == 1)
        {
            return new NumSys(input.Length.ToString(), 10);
        }    
    
        // Working with negative value
        bool isNegative = input.IsNegative();
        if (isNegative) input = input.Replace("-", "");
    
        /*
        Adding trailing zeros on fractional part.
        Without them everything will be broken
        */
        input = input.AddTrailingZeros();
  
        string[] parts = input.Split(',', '.');
        string integerPart = parts[0];
        string fractionalPart = parts[1];
        decimal newIntegerStore = 0;
        decimal newFractionalStore = 0;
        foreach (char i in integerPart)
        {
            decimal a = Convert.ToDecimal(Alphabet.IndexOf(i));
            decimal b = a * (decimal)(Math.Pow(origBase, Math.Abs((integerPart.IndexOf(i) - (integerPart.Length - 1)))));
            newIntegerStore += b;
        }

        foreach (char i in fractionalPart)
        {
            int a = Alphabet.IndexOf(i);
            decimal b = (decimal)(a * Math.Pow(origBase, (-(fractionalPart.IndexOf(i) + 1))));
            newFractionalStore += b;
        }
        // Returning value with(out) minus

        return isNegative ? new NumSys(Decimal.Negate(newIntegerStore + newFractionalStore).ToString(CultureInfo.InvariantCulture), 10)
            : new NumSys((newIntegerStore + newFractionalStore).ToString(CultureInfo.InvariantCulture), 10);
    }
    
    public NumSys FromDecimalToAnyBase(int newBase, int accuracy = 10)
    {
        if (this.NumberBase != 10) 
            throw new ArgumentException("The FromDecimalToAnyBase method allows NumSys objects only with NumSys.NumberBase = 10");
        if (!(newBase >= 1 && newBase <=50)) 
            throw new ArgumentException("The base should be lower than 50 and higher than 0");
        // Working with negative value
        decimal input = decimal.Parse(this.Number);
        bool isNegative = Decimal.IsNegative(input); 
        if (isNegative) input = Math.Abs(input);
        
        // Working with unary number system again
        if (newBase == 1)
        {
            return new NumSys("1".Repeat((int)input), 1);
        }    
        
        /*
        We need to specify our accuracy while converting decimal argument to string.
        In this case we will use 10 digits after comma
        */
        string[] parts = input.ToString($"F{accuracy}").Split(',', '.');
        string inputIntegerPart = parts[0];
        string inputFractPart = "0." + parts[1];
        List<char> newFractionalPartList = new List<char>();
        List<char> newIntegerPartList = new List<char>();
        int oldIntegerPart = int.Parse(inputIntegerPart);
        decimal oldFractionalPart = Convert.ToDecimal(inputFractPart); 
        
        // Working with integer part of decimalParam
        while (oldIntegerPart > 0)
        {
            newIntegerPartList.Add(Alphabet[oldIntegerPart % newBase]);
            oldIntegerPart /= newBase;
        }
        // Reversing the new integer part
        newIntegerPartList.Reverse();
    
        // Working with fractional part of decimalParam
        // Accuracy will be around 10 symbols after a comma
        int counter = 0;
        while (oldFractionalPart != 0 && counter != accuracy)
        {
            decimal stepOne = oldFractionalPart * newBase;
            newFractionalPartList.Add(Alphabet[(int)stepOne]);
            counter++;
            oldFractionalPart = stepOne - (int)stepOne;
        }
        
        
        // Making sure that won't be empty if original inputs fractional part equals to zero
        if (counter == 0) newFractionalPartList.Add('0');
        String.Join("", newIntegerPartList);
        // Returning combination of integer and fractional parts, plus minus if needed
        if (isNegative) return new NumSys("-" + String.Join("", newIntegerPartList) + "." + String.Join("", newFractionalPartList), newBase);
        return new NumSys((String.Join("", newIntegerPartList) + "." + String.Join("", newFractionalPartList)).RemoveTrailingZeros(), newBase);
    }

    public NumSys ConvertToAnyBase(int newBase)
    {
        if (!(newBase >= 1 && newBase <=50)) 
            throw new ArgumentException("The base should be lower than 50 and higher than 0");
        
        return this.FromAnyBaseToDecimal().FromDecimalToAnyBase(newBase);
    }

    public NumSys ConvertTo(int newBase)
    {
        return new NumSys(_number, newBase);
    }
    
    public static NumSys Sum(NumSys number1, NumSys number2)
    {
        if (number1.NumberBase != number2.NumberBase)
            throw new ArgumentException("The bases of two passed numbers must be equal");
        int bases = number1.NumberBase;
        string strNumber1 = number1.Number;
        string strNumber2 = number2.Number;
        var sbNumber1 = new StringBuilder(strNumber1);
        if (!strNumber1.Contains('.') || !strNumber1.Contains(',')) sbNumber1.Append(".0");
        sbNumber1.Replace(',', '.');
        var sbNumber2 = new StringBuilder(strNumber2);
        if (!strNumber2.Contains('.') || !strNumber2.Contains(',')) sbNumber2.Append(".0");
        sbNumber2.Replace(',', '.');
        List<char> result = new List<char>();
        
        /*
        Firstly, we should make sure that both numbers have the same length.
        It can be achieved by adding zeros.
        */

        sbNumber1.AddTrailingZeros();
        int indexOfCommaNumber1 = sbNumber1.IndexOfChar('.');
        int lengthBeforeCommaNumber1 = sbNumber1.Substring(0,indexOfCommaNumber1 - 1).Length;
        int lengthAfterCommaNumber1 = sbNumber1.Substring(indexOfCommaNumber1 + 1).Length;
        
        
        sbNumber2.AddTrailingZeros();
        int indexOfCommaNumber2 = sbNumber2.IndexOfChar('.');
        int lengthBeforeCommaNumber2 = sbNumber2.Substring(0,indexOfCommaNumber2 - 1).Length;
        int lengthAfterCommaNumber2 = sbNumber2.Substring(indexOfCommaNumber2 + 1).Length;
        
        // Actually, because we have 2 parts of every string (fractinal and integer) the process of making them 
        // 'equal' will be pretty complicated and will include multiple branches
        if (sbNumber1.Length != sbNumber2.Length)
        {
            if (sbNumber1.Length > sbNumber2.Length)
            {
                sbNumber2.Insert(0, "0".Repeat(strNumber1.Length - strNumber2.Length));
            }
            else
            {
                sbNumber1.Insert(0, "0".Repeat(strNumber2.Length - strNumber1.Length));
            }
        }

        if (sbNumber1.Length != sbNumber2.Length)
        {
            if (lengthBeforeCommaNumber1 != lengthBeforeCommaNumber2)
            {
                if (lengthBeforeCommaNumber1 > lengthBeforeCommaNumber2)
                {
                    sbNumber2.Insert(0, "0".Repeat(lengthBeforeCommaNumber1 - lengthBeforeCommaNumber2));
                }
                else
                {
                    sbNumber1.Insert(0, "0".Repeat(lengthBeforeCommaNumber2 - lengthBeforeCommaNumber1));
                }
            }

            if (lengthAfterCommaNumber1 != lengthAfterCommaNumber2)
            {
                if (lengthAfterCommaNumber1 > lengthAfterCommaNumber2)
                {
                    sbNumber2.Insert(sbNumber2.Length - 1, "0".Repeat(lengthAfterCommaNumber1 - lengthAfterCommaNumber2));
                }
                else
                {
                    sbNumber1.Insert(sbNumber1.Length - 1, "0".Repeat(lengthAfterCommaNumber2 - lengthAfterCommaNumber1));
                }
            }
        }
        
        // After changing both sbNumber1 & sbNumber2 and adding zeros to them, the index of comma should be the same for both of them
        int indexOfCommaBothNumbers = sbNumber1.IndexOfChar('.');
        
        // Removing commas in both numbers
        sbNumber1.Remove(indexOfCommaBothNumbers, 1);
        sbNumber2.Remove(indexOfCommaBothNumbers, 1);
        
        
        int[] carry = new int[sbNumber1.Length + 1];
        
        for (int i = sbNumber1.Length - 1; i > -1; i--)
        {
            int sumChar = Alphabet.IndexOf(sbNumber1[i]) + Alphabet.IndexOf(sbNumber2[i]);
            sumChar += carry[i+1];
            int carryChar = sumChar / bases;
            carry[i] += carryChar;
            if (sumChar >= bases)
            {
                sumChar %= bases;
            }
            result.Add(Alphabet[sumChar]);
        }

        if (carry[0] != 0)
        {
            result.Add(Alphabet[carry[0]]);
        }

        result.Reverse();
        result.Insert(indexOfCommaBothNumbers + 1, '.');
        
        // Returning the result and optionally getting rif of trailing zeros
        
        string trueResult = string.Concat(result);
        
        return new NumSys(trueResult.RemoveTrailingZeros(), number1.NumberBase);
    }
    private static string StringSum(in string number1, in string number2, in int bases)
    {
        var sbNumber1 = new StringBuilder(number1);
        if (!number1.Contains('.') || !number1.Contains(',')) sbNumber1.Append(".0");
        sbNumber1.Replace(',', '.');
        var sbNumber2 = new StringBuilder(number2);
        if (!number2.Contains('.') || !number2.Contains(',')) sbNumber2.Append(".0");
        sbNumber2.Replace(',', '.');
        List<char> result = new List<char>();
        
        /*
        Firstly, we should make sure that both numbers have the same length.
        It can be achieved by adding zeros.
        */

        sbNumber1.AddTrailingZeros();
        int indexOfCommaNumber1 = sbNumber1.IndexOfChar('.');
        int lengthBeforeCommaNumber1 = sbNumber1.Substring(0,indexOfCommaNumber1 - 1).Length;
        int lengthAfterCommaNumber1 = sbNumber1.Substring(indexOfCommaNumber1 + 1).Length;
        
        
        sbNumber2.AddTrailingZeros();
        int indexOfCommaNumber2 = sbNumber2.IndexOfChar('.');
        int lengthBeforeCommaNumber2 = sbNumber2.Substring(0,indexOfCommaNumber2 - 1).Length;
        int lengthAfterCommaNumber2 = sbNumber2.Substring(indexOfCommaNumber2 + 1).Length;
        
        // Actually, because we have 2 parts of every string (fractinal and integer) the process of making them 
        // 'equal' will be pretty complicated and will include multiple branches
        if (sbNumber1.Length != sbNumber2.Length)
        {
            if (sbNumber1.Length > sbNumber2.Length)
            {
                sbNumber2.Insert(0, "0".Repeat(number1.Length - number2.Length));
            }
            else
            {
                sbNumber1.Insert(0, "0".Repeat(number2.Length - number1.Length));
            }
        }

        if (sbNumber1.Length != sbNumber2.Length)
        {
            if (lengthBeforeCommaNumber1 != lengthBeforeCommaNumber2)
            {
                if (lengthBeforeCommaNumber1 > lengthBeforeCommaNumber2)
                {
                    sbNumber2.Insert(0, "0".Repeat(lengthBeforeCommaNumber1 - lengthBeforeCommaNumber2));
                }
                else
                {
                    sbNumber1.Insert(0, "0".Repeat(lengthBeforeCommaNumber2 - lengthBeforeCommaNumber1));
                }
            }

            if (lengthAfterCommaNumber1 != lengthAfterCommaNumber2)
            {
                if (lengthAfterCommaNumber1 > lengthAfterCommaNumber2)
                {
                    sbNumber2.Insert(sbNumber2.Length - 1, "0".Repeat(lengthAfterCommaNumber1 - lengthAfterCommaNumber2));
                }
                else
                {
                    sbNumber1.Insert(sbNumber1.Length - 1, "0".Repeat(lengthAfterCommaNumber2 - lengthAfterCommaNumber1));
                }
            }
        }
        
        // After changing both sbNumber1 & sbNumber2 and adding zeros to them, the index of comma should be the same for both of them
        int indexOfCommaBothNumbers = sbNumber1.IndexOfChar('.');
        
        // Removing commas in both numbers
        sbNumber1.Remove(indexOfCommaBothNumbers, 1);
        sbNumber2.Remove(indexOfCommaBothNumbers, 1);
        
        
        int[] carry = new int[sbNumber1.Length + 1];
        
        for (int i = sbNumber1.Length - 1; i > -1; i--)
        {
            int sumChar = Alphabet.IndexOf(sbNumber1[i]) + Alphabet.IndexOf(sbNumber2[i]);
            sumChar += carry[i+1];
            int carryChar = sumChar / bases;
            carry[i] += carryChar;
            if (sumChar >= bases)
            {
                sumChar %= bases;
            }
            result.Add(Alphabet[sumChar]);
        }

        if (carry[0] != 0)
        {
            result.Add(Alphabet[carry[0]]);
        }

        result.Reverse();
        result.Insert(indexOfCommaBothNumbers, '.');
        
        // Returning the result and optionally getting rif of trailing zeros
        
        string trueResult = string.Concat(result);
        
        return trueResult.RemoveTrailingZeros();
    }

    public static NumSys Multiply(NumSys number1, NumSys number2)
    {
        if (number1.NumberBase != number2.NumberBase)
            throw new ArgumentException("The bases of two passed numbers must be equal");
        
        string strNumber1 = number1.Number;
        string strNumber2 = number2.Number;
        int bases = number1.NumberBase;
        bool resultHasMinus = (strNumber1.IsNegative() && strNumber2.IsPositive()) 
                              || (strNumber1.IsPositive() && strNumber2.IsNegative());
        strNumber1 = strNumber1.Replace(',', '.');
        strNumber2 = strNumber2.Replace(',', '.');

        strNumber1 = strNumber1.AddTrailingZeros();
        strNumber2 = strNumber2.AddTrailingZeros();
        
        int indexOfCommaNumber1 = strNumber1.IndexOf('.');
        int indexOfCommaNumber2 = strNumber2.IndexOf('.');
        
        int lengthAfterCommaNumber = strNumber1.Substring(indexOfCommaNumber1 + 1).Length + strNumber2.Substring(indexOfCommaNumber2 + 1).Length;

        strNumber1 = strNumber1.Remove(indexOfCommaNumber1, 1);
        strNumber2 = strNumber2.Remove(indexOfCommaNumber2, 1);
        
        
        
        
        StringBuilder[] stringsForSum = new StringBuilder[strNumber2.Length];
        int stringCounter = 0;
        for (int i = strNumber2.Length - 1; i > -1; i--)
        {
            List<char> tempString = new List<char>();
            int[] carry = new int[strNumber1.Length + 1]; 
            
            for (int j = strNumber1.Length - 1; j > -1; j--)
            {
                int multChar = Alphabet.IndexOf(strNumber2[i]) * Alphabet.IndexOf(strNumber1[j]);
                multChar += carry[j+1];
                int carryChar = multChar / bases;
                carry[j] += carryChar;
                if (multChar >= bases)
                {
                    multChar -= bases * (carryChar);
                }
                tempString.Add(Alphabet[multChar]);
            }
            if (carry[0] != 0)
            {
                tempString.Add(Alphabet[carry[0]]);
            }

            tempString.Reverse();
            
            stringsForSum[Math.Abs(i - (strNumber2.Length - 1))] = new StringBuilder(String.Concat(tempString) + string.Concat(Enumerable.Repeat("0", stringCounter)));
            
            stringCounter += 1;

        }
        
        string resultOfSumStrings = "0";
        
        foreach (var item in stringsForSum)
        { 
            resultOfSumStrings = NumSys.StringSum(resultOfSumStrings, item.ToString(), bases);
        }
        
        resultOfSumStrings = resultOfSumStrings.Insert(resultOfSumStrings.Length - lengthAfterCommaNumber, ".");

        resultOfSumStrings = resultOfSumStrings.RemoveTrailingZeros();
        
        return resultHasMinus ? 
            new NumSys(resultOfSumStrings.Insert(0, "-"), number1.NumberBase) :
            new NumSys(resultOfSumStrings, number1.NumberBase);
        
    }
}

static class SyntaxParse
{
    public static string HelpText = @"
    This is a simple calculator which processes users input with some commands and returns a result.
    Pseudosyntax for the input is simple: any valid input consists of three expressions: number, command and base (optional) separated by one space.
    Numbers should be written with addition of ^^base part in the end. Example: ab^^16
    Commands are: +, *, => . The last one stands for converting number to other number systems. Example: ab^^16 => 17
    !!!Keep in mind that commands are executed in direct order. Arithmetics rules are not implemented yet.
    Base are represented by integer numbers in this field (1 <= x <= 50).
    Summing up, correct input should look like this: ab^^16 + ca^^16 * cc^^16 => 18 .
    There are also some other useful command:
    -alphabet - return symbol alphabet used by this app to work with different number systems.
    -exit - exits calculator.";
                                  
    private static string _numberPattern = @"^(?!-+$)-?[0-9a-zA-MN,.]+?\^\^([1-9]|[1-4][0-9]|50)$";
    private static string _commandList = "*|+|=>";
    private static string _basePattern = @"^(50|[1-9]|[1-4][0-9])$";

    public static bool IsValidCommand(string command)
    {
        if (_commandList.Contains(command)) return true;
        return false;
    }

    public static bool IsValidNumber(string number)
    {
        if (Regex.IsMatch(number, _numberPattern)) return true;
        return false;
    }

    public static bool IsValidBase(string bases)
    {
        if (!Regex.IsMatch(bases, _basePattern)) return false;
        if (int.Parse(bases) >= 1 && int.Parse(bases) <= 50) return true;
        return false;
    }

    public static NumSys ToNumSys(string number)
    {
        if (!number.Contains("^^")) 
            throw new Exception("The number parameter for ToNumSys method should contain only one pair of ^^ symbols");
        string[] numAndBase = number.Split("^^");
        return NumSys.Parse(numAndBase[0], int.Parse(numAndBase[1]));
    }

    public static bool IsValidInput(string input)
    {
        string[] dissected = input.Split(' ');
        int validCommandCounter = 0;
        int validNumberCounter = 0;
        foreach (var element in dissected)
        {
            if (IsValidCommand(element)) validCommandCounter++;
            if (IsValidNumber(element)) validNumberCounter++;
            if (IsValidBase(element)) validNumberCounter++;
        }
        return (validNumberCounter + validCommandCounter == dissected.Length) &&
               (validNumberCounter == validCommandCounter + 1) && (IsValidNumber(dissected[0])) 
               && ((IsValidNumber(dissected[^1]) || IsValidBase(dissected[^1])));
    }

    // this is so stupid but idk what to do
    public static NumSys ExecuteInput(string input)
    {
        string[] dissected = input.Split(' ');
        List<string> expressionList = dissected.ToList();
        while (expressionList.Count != 1)
        {
            string tempStoreForNumber1 = expressionList[0];
            string tempStoreForCommand = expressionList[1];
            string tempStoreForNumber2 = expressionList[2]; // Also can store base , as intended
            string tempResult = "smth went wrong if you see this";
            if (tempStoreForCommand == "+") tempResult = NumSys.Sum(ToNumSys(tempStoreForNumber1), ToNumSys(tempStoreForNumber2)).ToString();
            if (tempStoreForCommand == "*") tempResult = NumSys.Multiply(ToNumSys(tempStoreForNumber1), ToNumSys(tempStoreForNumber2)).ToString();
            if (tempStoreForCommand == "=>") tempResult = ToNumSys(tempStoreForNumber1).ConvertToAnyBase(int.Parse(tempStoreForNumber2)).ToString();
            if ((tempStoreForCommand != "+") && (tempStoreForCommand != "*") && (tempStoreForCommand != "=>"))
                throw new ArgumentException("The command between some 2 numbers is invalid, check syntax");
            
            expressionList.RemoveRange(0, 3);
            expressionList.Insert(0, tempResult);
        }
        
        return ToNumSys(expressionList[0]);
    }
}

class Program
{
    static void Main()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        
        bool exitStatus = false;

        do
        {
            Console.Write(">");
            var input = Console.ReadLine();
            input = input.Trim();
            if (input.ToLower() == "exit") exitStatus = true;
            else if (input.ToLower() == "alphabet") Console.WriteLine(NumSys.Alphabet);
            else if (input.ToLower() == "help" || input == "?") Console.WriteLine(SyntaxParse.HelpText);
            else if (SyntaxParse.IsValidInput(input))
                try
                {
                    Console.WriteLine(SyntaxParse.ExecuteInput(input));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("The input hasn't been properly processed because: " + ex.Message);
                }
            else Console.WriteLine("Wasn't able to process the input. Type 'help' or '?' for more information.");
        } while (!exitStatus);
        
// Old input processing, might be useful in the future
/*
bool inputIsValid;
string? number;
int origNumSystem = 0;
int newNumSystem = 0;

do
{
    Console.Write("Enter your number: ");
    number = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(number) || !NumSys.NumberCheck(number))
    {
        Console.WriteLine("You must enter a valid number.");
        inputIsValid = false;
        continue;
    }

    Console.Write("Enter original number system: ");
    string? inputOrigNumSystem = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(inputOrigNumSystem) || !NumSys.NewBaseCheck(inputOrigNumSystem))
    {
        Console.WriteLine("Invalid input for original number system." +
                          " The input should be an integer. Try again from the very beginning");
        inputIsValid = false;
        continue;
    }


    Console.Write("Enter new number system: ");
    string? inputNewNumSystem = Console.ReadLine();
    if (string.IsNullOrEmpty(inputNewNumSystem) || !NumSys.OrigBaseCheck(inputNewNumSystem))
    {
        Console.WriteLine("Invalid input for new number system. The input should be an integer. " +
                          "Try again from the very beginning");
        inputIsValid = false;
        continue;
    }

    inputIsValid = true;
    origNumSystem = int.Parse(inputOrigNumSystem);
    newNumSystem = int.Parse(inputNewNumSystem);

} while (!inputIsValid);
*/
        //var result = NumSys.ToAnything(NumSys.ToDecimal(number, origNumSystem), newNumSystem);

        /*
        var num1 = NumSys.Parse("ab12.30c", 16);
        var num2 = NumSys.Parse("fc.41", 16);
        var result = NumSys.Multiply(num1, num2);
        Console.WriteLine($"The result is {result}");

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
        */
    }
}