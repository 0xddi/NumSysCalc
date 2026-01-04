using System.Text.RegularExpressions;

namespace NumSysCalc;

public enum Mode
{
    NumSys,
    Cpu
}
public class SyntaxParser
{
    public static string HelpText = @"
    This is a simple calculator which processes users input with some commands and returns a result.
    It works in 2 modes: NumSys (by default) and CPU.

    You can change these two modes by typing: 
    'set mode numsys'
    'set mode cpu'

    In NumSys mode, pseudosyntax for the input is simple: any valid input consists of three expressions: number, command and base (optional) separated by one space.
    Numbers should be written with addition of ^^base part in the end. Example: ab^^16
    Commands are: +, *, => . The last one stands for converting number to other number systems. Example: ab^^16 => 17
    !!!Keep in mind that commands are executed in direct order. Arithmetics rules are not implemented yet.
    Base are represented by integer numbers in this field (1 <= x <= 50).
    Summing up, correct input should look like this: 'ab^^16 + ca^^16 * cc^^16 => 18' .

    In CPU mode, there are only 4 available commands with one decimal operand after them at single time.
    'tsmr 10' - returns signed magnitude representation of an operand (прямой код);
    'toc 10' - returns ones complement representation of an operand (обратный код);
    'ttc 10' - returns twos complement representation of an operand (дополнительный код);
    'tof 10' - returns float representation of an operand in IEEE 754 single standard (представление вещественных чисел в ЭВМ);

    There are also some other useful command:
    'alphabet' - returns symbol alphabet used by this app to work with different number systems.
    'exit' - exits calculator.";
                                  
    private static string _numberPatternNumSys = @"^(?!-+$)-?[0-9a-zA-MN,.]+?\^\^([1-9]|[1-4][0-9]|50)$";
    private static string _numberPatternCpu = @"^-?(0|[1-9]\d*)(\.\d+)?$";
    private static string _commandList = "*|+|=>";
    private static string _basePattern = @"^(50|[1-9]|[1-4][0-9])$";

    public static bool IsValidCommand(string command)
    {
        if (_commandList.Contains(command)) return true;
        return false;
    }

    public static bool IsValidNumber(string number)
    {
        if (Regex.IsMatch(number, _numberPatternNumSys)) return true;
        return false;
    }

    public static bool IsValidBase(string bases)
    {
        if (!Regex.IsMatch(bases, _basePattern)) return false;
        if (int.Parse(bases) >= 1 && int.Parse(bases) <= 50) return true;
        return false;
    }

    public static Number ToNumber(string number)
    {
        byte numberSign;
        if (!number.Contains("^^")) 
            throw new Exception("The number parameter for ConvertToNumber method should contain only one pair of ^^ symbols");
        string[] numberBodyAndBase = number.Split("^^");
        string newBody = numberBodyAndBase[0];
        string numberBase = numberBodyAndBase[1];
        if (newBody[0] == '-')
        {
            numberSign = 1;
            newBody = newBody.Remove(0, 1);
        }
        else numberSign = 0;
        return new Number(numberSign, newBody, int.Parse(numberBase));
    }

    public static bool IsValidNumSysInput(string input)
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
    
    public static bool IsValidCpuInput(string input)
    {
        string[] dissected = input.Split(' ');
        if (dissected.Length != 2) return false;
        if ((dissected[0].ToLower() == "tfr" || dissected[0].ToLower() == "tsmr" || dissected[0].ToLower() == "toc" || dissected[0].ToLower() == "ttc") && Regex.IsMatch(dissected[1], _numberPatternCpu))
            return true;
        return false;
    }
    // this is so stupid but ok
    public static Number ExecuteNumSysInput(string input)
    {
        string[] dissected = input.Split(' ');
        List<string> expressionList = dissected.ToList();
        while (expressionList.Count != 1)
        {
            string tempStoreForNumber1 = expressionList[0];
            string tempStoreForCommand = expressionList[1];
            string tempStoreForNumber2 = expressionList[2]; // Also can store base , as intended
            string tempResult = "smth went wrong if you see this";
            if (tempStoreForCommand == "+") tempResult = Number.Sum(ToNumber(tempStoreForNumber1), ToNumber(tempStoreForNumber2)).ToString();
            if (tempStoreForCommand == "*") tempResult = Number.Multiply(ToNumber(tempStoreForNumber1), ToNumber(tempStoreForNumber2)).ToString();
            if (tempStoreForCommand == "=>") tempResult = ToNumber(tempStoreForNumber1).ConvertToAnyBase(int.Parse(tempStoreForNumber2)).ToString();
            if ((tempStoreForCommand != "+") && (tempStoreForCommand != "*") && (tempStoreForCommand != "=>"))
                throw new ArgumentException("The command between some 2 numbers is invalid, check syntax");
            
            expressionList.RemoveRange(0, 3);
            expressionList.Insert(0, tempResult);
        }
        
        return ToNumber(expressionList[0]);
    }

    public static string ExecuteCpuInput(string input)
    {
        string[] dissected = input.Split(' ');
        if (dissected[0] == "tsmr") return CPURepresentation.ToSignedMagnitudeRepresentation(dissected[1]);
        if (dissected[0] == "toc") return CPURepresentation.ToOnesComplement(dissected[1]);
        if (dissected[0] == "ttc") return CPURepresentation.ToTwosComplement(dissected[1]);
        if (dissected[0] == "tfr") return CPURepresentation.ToFloatRepresentation(dissected[1]);
        throw new ArgumentException("The command couldn't be processed normally. Check for typos etc.");
    }
}