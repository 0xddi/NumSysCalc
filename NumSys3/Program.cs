using System.Text.RegularExpressions;
namespace NumSys3;
static class NumSys
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMN";
    private const string InputPattern = @"^(?!-+$)(?!.*-.*-)[0-9a-zA-MN\-,.]+$";
    
    public static decimal ToDecimal(string input, in int origBase)
    {
        // No comments, too obvious
        if (origBase == 10)
        {
            return Convert.ToDecimal(input, System.Globalization.CultureInfo.InvariantCulture);
        } 
    
        // Working with unary number system
        if (origBase == 1)
        {
            return input.Count(c => c == '1');
        }    
    
        // Working with negative value
        bool isNegative = false;
        if (input.Contains('-'))
        {
            input = input.Replace("-", "");
            isNegative = true;
        }    
    
        /*
        Adding trailing zeros on fractional part.
        Without them everything will be broken
        */
        if (!input.Contains('.') && !input.Contains(','))
        {
            input += ".0";
        }    
        string[] parts = input.Split(',', '.');
        string integerPart = parts[0];
        string fractionalPart = parts[1];
        decimal integerStore = 0;
        decimal fractionalStore = 0;
        foreach (char i in integerPart)
        {
            decimal a = Convert.ToDecimal(Alphabet.IndexOf(i), System.Globalization.CultureInfo.InvariantCulture);
            decimal b = a * (decimal)(Math.Pow(origBase, Math.Abs((integerPart.IndexOf(i) - (integerPart.Length - 1)))));
            integerStore += b;
        }

        foreach (char i in fractionalPart)
        {
            int a = Alphabet.IndexOf(i);
            decimal b = (decimal)(a * Math.Pow(origBase, (-(fractionalPart.IndexOf(i) + 1))));
            fractionalStore += b;
        }
        // Returning value with(out) minus
        if (isNegative)
        {
            return Decimal.Negate(integerStore + fractionalStore);
        }
        return integerStore + fractionalStore;
    }
    
    public static string ToAnything(decimal decimalParam, in int newBase)
    {
        // Working with negative value
        bool isNegative = Decimal.IsNegative(decimalParam); 
        if (isNegative) decimalParam = Math.Abs(decimalParam);
        
        // Working with unary number system again
        if (newBase == 1)
        {
            return string.Concat(Enumerable.Repeat("1", (int)decimalParam));
        }    
        
        /*
        We need to specify our accuracy while converting decimal argument to string.
        In this case we will use 10 digits after comma
        */
        string[] parts = decimalParam.ToString("F10", System.Globalization.CultureInfo.InvariantCulture).Split(',', '.');
        string decimalIntPart = parts[0];
        string decimalFractPart = "0." + parts[1];
        string integerStore = "";
        string fractionalStore = "";
        int intDecimalIntPart = int.Parse(decimalIntPart);
        decimal mDecimalFractPart = Convert.ToDecimal(decimalFractPart, System.Globalization.CultureInfo.InvariantCulture); 
        string reversedIntegerStore = "";
        
        // Working with integer part of decimalParam
        while (intDecimalIntPart > 0)
        {
            integerStore += Alphabet[(intDecimalIntPart % newBase)].ToString();
            intDecimalIntPart /= newBase;
        }
        // Reversing the integerStore string (still integer part)
        for (int i = integerStore.Length - 1; i > -1; i--)
        {
             reversedIntegerStore += integerStore[i];
        } 
    
        // Working with fractional part of decimalParam
        // Accuracy will be around 10 symbols after a comma
        int counter = 0;
        while (mDecimalFractPart != 0 && counter != 10)
        {
            decimal stepOne = mDecimalFractPart * newBase;
            fractionalStore += Alphabet[(int)stepOne].ToString(); 
            counter++;
            mDecimalFractPart = stepOne - (int)stepOne;
        }
    
        // Returning combination of integer and fractional parts, plus minus if needed
        if (isNegative) return "-" + reversedIntegerStore + "." + fractionalStore;
        return reversedIntegerStore + "." + fractionalStore;
    }

    public static bool NumberCheck(string inputNumber)
    {
        return Regex.IsMatch(inputNumber, InputPattern);
    }

    public static bool OrigBaseCheck(string inputOrigBase)
    {
        
        return int.TryParse(inputOrigBase, out int origBaseTemp) && (1 <= origBaseTemp) 
                                                                 && (origBaseTemp <= 50);
    }
    
    public static bool NewBaseCheck(string inputNewBase)
    {
        
        return int.TryParse(inputNewBase, out int newBaseTemp) && (1 <= newBaseTemp) 
                                                                 && (newBaseTemp <= 50);
    }
}

class Program
{
    static void Main()
    {
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
        
        var result = NumSys.ToAnything(NumSys.ToDecimal(number, origNumSystem), newNumSystem);
        Console.WriteLine($"The result is {result}");
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}