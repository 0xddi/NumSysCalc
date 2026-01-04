using System.Globalization;
namespace NumSysCalc;

class Program
{
    static void Main()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        
        bool exitStatus = false;
        Mode currentMode = Mode.NumSys;
        do
        {
            Console.Write(">");
            var input = Console.ReadLine()!;
            input = input.Trim();
            if (input.ToLower() == "exit") exitStatus = true;
            else if (input.ToLower() == "set mode numsys")
            {
                currentMode = Mode.NumSys;
                Console.WriteLine("The current mode has been successfully set to NumSys mode");
            }
            else if (input.ToLower() == "set mode cpu")
            {
                currentMode = Mode.Cpu;
                Console.WriteLine("The current mode has been successfully set to CPU mode");
            }
            else if (input.ToLower() == "alphabet") Console.WriteLine(Number.Alphabet);
            else if (input.ToLower() == "help" || input == "?") Console.WriteLine(SyntaxParser.HelpText);
            else if (currentMode == Mode.NumSys && SyntaxParser.IsValidNumSysInput(input))
                try
                {
                    Console.WriteLine(SyntaxParser.ExecuteNumSysInput(input));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("The input hasn't been properly processed in NumSys mode because: " + ex.Message);
                }
            else if (currentMode == Mode.Cpu && SyntaxParser.IsValidCpuInput(input))
                try
                {
                    Console.WriteLine(SyntaxParser.ExecuteCpuInput(input));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("The input hasn't been properly processed in CPU mode because: " + ex.Message);
                }
            else Console.WriteLine("Wasn't able to process the input in both modes. Type 'help' or '?' for more information.");
        } while (!exitStatus);
    }
}