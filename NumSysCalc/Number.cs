using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NumSysCalc;

public class Number
{
    
    public const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMN";
    private const string InputPattern = @"^(?!-+$)(?!.*-.*-)[0-9a-zA-MN\-,.]+$";
    
    
    public string NumberBody { get; private set;}
    public int NumberBase { get; private set;}
    public byte NumberSign { get; private set;}
    
    public Number(byte numberSign, string numberBody, int numberBase)
    {
        if (Regex.IsMatch(numberBody, InputPattern)) this.NumberBody = numberBody;
        else throw new ArgumentException("The number should contain only alphabet characters and one minus at the beginning");
        if (numberBase >= 1 && numberBase <= 50) this.NumberBase = numberBase;
        else throw new ArgumentException("The base should be lower than 50 and higher than 0");
        if (numberSign == 0 || numberSign == 1) this.NumberSign = numberSign;
        else throw new ArgumentException("The sign should consist of only 0 (if positive) or 1 (if negative)");
    }
    
    public override string ToString()
    {
        string strSign;
        if (this.NumberSign == 1) strSign = "-";
        else strSign = "";
        return strSign + this.NumberBody + "^^" + this.NumberBase.ToString();
    }
    
    
    public Number FromAnyBaseToDecimal()
    {
        Console.WriteLine($"Начинаем процесс перевода числа {this.NumberSign}{this.NumberBody} из {this.NumberBase} с.с. в десятичную.");
        string newNumberBody = this.NumberBody;
        int origBase = this.NumberBase;
        byte origSign = this.NumberSign;
        // No comments, too obvious
        if (origBase == 10)
        {
            Console.WriteLine(@"
            Заметим, что данное нам число уже записано в десятичной системе счисления. Следовательно,
            нам не нужны какие-либо преобразования и мы можем сразу перейти к другому этапу.");
            return this;
        } 
    
        // Working with unary number system
        if (origBase == 1)
        {
            Console.WriteLine(@"
            Заметим, что данное нам число записано в унарной системе счисления. Унарная с. с.
            является непозиционной и имеет свой уникальный алгоритм перевода. Для нахождения 
            десятичной записи нам всего лишь надо посчитать количество единиц в данном числе и
            и записать это самое количество в десятичном виде.");
            // Sign is always positive (=0) because of unary system
            return new Number(0,newNumberBody.Length.ToString(), 10);
        }    
        
    
        /*
        Adding trailing zeros on fractional part.
        Without them everything will be broken
        */
        newNumberBody = newNumberBody.AddTrailingZeros();
  
        string[] parts = newNumberBody.Split(',', '.');
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
        Console.WriteLine(@"
        Здесь используем классическую схему перевода из n-ной с.с. в десятичную.
        В рамках целой части начинаем с самой правой цифры: умножаем это число на
        основание с.с. в степени индекса, который начинается с нуля.");
        Console.WriteLine($"Получили в 'переведённой' целой части: {newIntegerStore}");

        int position = 1;
        foreach (char i in fractionalPart)
        {
            int a = Alphabet.IndexOf(i);
            decimal b = (decimal)(a * Math.Pow(origBase, -position));
            newFractionalStore += b;
            position++;
        }
        
        Console.WriteLine(@"
        Для нецелой части алгоритм идентичный. Только начинаем с самой левой цифры, а 
        также индексу равному -1");
        Console.WriteLine($"Получили в 'переведённой' нецелой части: {newFractionalStore}");
        
        // Returning value with original sign
        Console.WriteLine($"Итог: {new Number(origSign, (newIntegerStore + newFractionalStore).ToString(CultureInfo.InvariantCulture), 10)}");
        return new Number(origSign, (newIntegerStore + newFractionalStore).ToString(CultureInfo.InvariantCulture), 10);
    }
    
    public Number FromDecimalToAnyBase(int newBase, int accuracy = 10)
    {
        Console.WriteLine($"Начинаем процесс перевода десятичного числа {this.NumberSign}{this.NumberBody} в {newBase} с.с.");
        byte origSign = this.NumberSign;
        if (this.NumberBase != 10) 
            throw new ArgumentException("The FromDecimalToAnyBase method allows NumSys objects only with NumSys.NumberBase = 10");
        if (!(newBase >= 1 && newBase <=50)) 
            throw new ArgumentException("The base should be lower than 50 and higher than 0");
        // 
        decimal newNumberBody = decimal.Parse(this.NumberBody);
        
        // Working with unary number system again
        if (newBase == 1)
        {
            return new Number(origSign, "1".Repeat((int)newNumberBody), 1);
        }    
        
        /*
        We need to specify our accuracy while converting decimal argument to string.
        In this case we will use 10 digits after comma
        */
        string[] parts = newNumberBody.ToString($"F{accuracy}").Split(',', '.');
        string inputIntegerPart = parts[0];
        string inputFractPart = "0." + parts[1];
        List<char> newFractionalPartList = new List<char>();
        List<char> newIntegerPartList = new List<char>();
        int oldIntegerPart = int.Parse(inputIntegerPart);
        decimal oldFractionalPart = Convert.ToDecimal(inputFractPart); 
        
        // Working with integer part of decimalParam
        Console.WriteLine(@"Переводим целую часть через столбик: путём последовательного
        деления этой самой целой части на основание новой с.с. и записью остатков, которую мы потом
        перевернём.");
        Console.WriteLine("");
        while (oldIntegerPart > 0)
        {
            newIntegerPartList.Add(Alphabet[oldIntegerPart % newBase]);
            oldIntegerPart /= newBase;
        }
        // Reversing the new integer part
        newIntegerPartList.Reverse();
        Console.WriteLine($"В целой части получили {String.Join("", newIntegerPartList)}");
    
        // Working with fractional part of decimalParam
        // Accuracy will be around 10 symbols after a comma
        Console.WriteLine(@"С переводом нецелой части ситуация иная. Мы просто должны умножать 
        изначальную нецелую часть на основание новой с.с., брать от этого целую часть и записывать 
        в новую нецелую часть, а затем отнимать от старой части вышеописанное произведение. Делаем так
        пока не получим 0 или не превысим 10 цифр (выбранный нами лимит).");
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
        Console.WriteLine($"Новая нецелая часть: {String.Join("", newFractionalPartList)}");
        String.Join("", newIntegerPartList);
        return new Number(origSign, (String.Join("", newIntegerPartList) + "." + String.Join("", newFractionalPartList)).RemoveTrailingZeros(), newBase);
    }
    
    public Number ConvertToAnyBase(int newBase)
    {
        if (!(newBase >= 1 && newBase <=50)) 
            throw new ArgumentException("The base should be lower than 50 and higher than 0");
        
        Console.WriteLine(@"
        Процесс перевода числа из одной системы счисления в другую систему счисления 
        можно представить в виде двух основных этапов: перевод из n-ой системы счисления
        в десятичную, перевод из десятичной в новую.");
        return this.FromAnyBaseToDecimal().FromDecimalToAnyBase(newBase);
    }
    
    public static Number Sum(Number number1, Number number2)
    {
        if (number1.NumberBase != number2.NumberBase)
            throw new ArgumentException("The bases of two passed numbers must be equal");
        int bases = number1.NumberBase;
        string strNumber1 = number1.NumberBody;
        string strNumber2 = number2.NumberBody;
        var sbNumber1 = new StringBuilder(strNumber1);
        if (!strNumber1.Contains('.') && !strNumber1.Contains(',')) sbNumber1.Append(".0");
        sbNumber1.Replace(',', '.');
        var sbNumber2 = new StringBuilder(strNumber2);
        if (!strNumber2.Contains('.') && !strNumber2.Contains(',')) sbNumber2.Append(".0");
        sbNumber2.Replace(',', '.');
        List<char> result = new List<char>();
        
        /*
        Firstly, we should make sure that both numbers have the same length.
        It can be achieved by adding zeros.
        */
        
        Console.WriteLine(@$"
        Начнём суммирование чисел {number1} b {number2}.Для начала нам нужно сделать их одинаковой длины.
        Сделаем это за счёт добавления нулей в конце.");

        sbNumber1.AddTrailingZeros();
        int indexOfCommaNumber1 = sbNumber1.IndexOfChar('.');
        int lengthBeforeCommaNumber1 = sbNumber1.Substring(0,indexOfCommaNumber1 - 1).Length;
        int lengthAfterCommaNumber1 = sbNumber1.Substring(indexOfCommaNumber1 + 1).Length;
        
        sbNumber2.AddTrailingZeros();
        int indexOfCommaNumber2 = sbNumber2.IndexOfChar('.');
        int lengthBeforeCommaNumber2 = sbNumber2.Substring(0,indexOfCommaNumber2 - 1).Length;
        int lengthAfterCommaNumber2 = sbNumber2.Substring(indexOfCommaNumber2 + 1).Length;
        
        // Actually, because we have 2 parts of every string (fractional and integer) the process of making them 
        // 'equal' will be pretty complicated and will include multiple branches

        if (lengthBeforeCommaNumber1 != lengthBeforeCommaNumber2 || lengthAfterCommaNumber1 != lengthAfterCommaNumber2)
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

        Console.WriteLine(@$"
        Получим числа {sbNumber1} и {sbNumber2}");
        
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
        
        Console.WriteLine(@"
        Путём обыкновенного сложения столбиком (если при суммировании чисел в одном столбце
        мы получаем число большее основания системы счисления, то мы записываем остаток деления в
        'дополнение наверху' к следующему столбцу, а разность (данного числа и остаток дел. * основание)
        записать непосредственно под двумя рассматриваемыми цифрами)");
        Console.WriteLine("Результат суммирования: " + trueResult);
        
        
        // Unfortunately works only with positive numbers
        Console.WriteLine($"Итог: {new Number(0, trueResult.RemoveTrailingZeros().RemoveUselessZeros(), number1.NumberBase)}");
        return new Number(0, trueResult.RemoveTrailingZeros().RemoveUselessZeros(), number1.NumberBase);
    }
    
    public static string StringSum(in string number1, in string number2, in int bases)
    {
        var sbNumber1 = new StringBuilder(number1);
        if (!number1.Contains('.') && !number1.Contains(',')) sbNumber1.Append(".0");
        sbNumber1.Replace(',', '.');
        var sbNumber2 = new StringBuilder(number2);
        if (!number2.Contains('.') && !number2.Contains(',')) sbNumber2.Append(".0");
        sbNumber2.Replace(',', '.');
        List<char> result = new List<char>();

        sbNumber1.AddTrailingZeros();
        int indexOfCommaNumber1 = sbNumber1.IndexOfChar('.');
        int lengthBeforeCommaNumber1 = sbNumber1.Substring(0,indexOfCommaNumber1 - 1).Length;
        int lengthAfterCommaNumber1 = sbNumber1.Substring(indexOfCommaNumber1 + 1).Length;
        
        sbNumber2.AddTrailingZeros();
        int indexOfCommaNumber2 = sbNumber2.IndexOfChar('.');
        int lengthBeforeCommaNumber2 = sbNumber2.Substring(0,indexOfCommaNumber2 - 1).Length;
        int lengthAfterCommaNumber2 = sbNumber2.Substring(indexOfCommaNumber2 + 1).Length;

        if (lengthBeforeCommaNumber1 != lengthBeforeCommaNumber2 || lengthAfterCommaNumber1 != lengthAfterCommaNumber2)
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
        
        string trueResult = string.Concat(result);
        
        return trueResult.RemoveTrailingZeros().RemoveUselessZeros();
    }
    
    public static Number Multiply(Number number1, Number number2)
    {
        byte newSign;
        if (number1.NumberBase != number2.NumberBase)
            throw new ArgumentException("The bases of two passed numbers must be equal");

        if (number1.NumberSign != number2.NumberSign) newSign = 1;
        else newSign = 0;
        
        string strNumber1 = number1.NumberBody;
        string strNumber2 = number2.NumberBody;
        int bases = number1.NumberBase;
        
        strNumber1 = strNumber1.Replace(',', '.');
        strNumber2 = strNumber2.Replace(',', '.');
        
        strNumber1 = strNumber1.Replace("-", "");
        strNumber2 = strNumber2.Replace("-", "");
        
        Console.WriteLine(@"
        Для начала мы временно избавляемся от возможных минусов в обоих числах
        Однако мы помним о наличии минуса в результате, если только одно из двух чисел отрицательно.");
        
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
        
        Console.WriteLine(@"
        Путём обыкновенного умножения столбиком числа первого на каждую цифру второго (если при умножении чисел в одном столбце
        мы получаем число большее основания системы счисления, то мы записываем остаток деления в
        дополнение к следующему столбцу, а разность (данного числа и остаток дел. * основание c. c.)
        записать непосредственно под двумя рассматриваемыми цифрами)");
        Console.WriteLine("Выпишем все полученные строки, которые потом просуммируем");
        foreach (var v in stringsForSum)
        {
            Console.WriteLine(v);
        }
        
        string resultOfSumStrings = "0";
        
        foreach (var item in stringsForSum)
        { 
            resultOfSumStrings = Number.StringSum(resultOfSumStrings, item.ToString(), bases);
        }
        
        resultOfSumStrings = resultOfSumStrings.Insert(resultOfSumStrings.Length - lengthAfterCommaNumber, ".");
        resultOfSumStrings = resultOfSumStrings.RemoveTrailingZeros().RemoveUselessZeros();
        
        return new Number(newSign, resultOfSumStrings, number1.NumberBase);
    }
}

