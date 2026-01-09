using System.Text;

namespace NumSysCalc;

public class CpuRepresentation
{
    private static Number ConvertToNumber(string str)
    {
        var strNum = str;
        byte numSign;
        if (strNum[0] == '-')
        {
            numSign = 1;
            strNum = strNum.Remove(0, 1);
        }
        else numSign = 0;

        return new Number(numSign, strNum, 10);
    }

    private static string ReverseBinary(string strNumber)
    {
        if (strNumber.Count(a => a == '0') + strNumber.Count(a => a == '1') != strNumber.Length) throw new Exception("The given binary representation contains invalid characters.");
        var tempStore = new StringBuilder(strNumber.Length);
        foreach (var digit in strNumber)
        {
            if (digit == '0') tempStore.Append('1');
            else tempStore.Append('0');
        }
        return tempStore.ToString();
    }
    
    // Конвертация десятичного числа в прямой код
    public static string ToSignedMagnitudeRepresentation(string strNum, int sizeOfBitField = 8)
    {
        Console.WriteLine("\n=== ПРЯМОЙ КОД (SIGNED MAGNITUDE) ===");
        Console.WriteLine("Теория: Прямой код - наиболее простой способ представления чисел в ЭВМ");
        Console.WriteLine("Правило: Старший бит - знак (0=+, 1=-), остальные биты - модуль числа");
        
        var tempStore = ConvertToNumber(strNum);
        var binaryStore = tempStore.FromDecimalToAnyBase(2);
        Console.WriteLine($"Шаг 1: Модуль числа в двоичной системе: {binaryStore.NumberBody}");

        if (binaryStore.NumberBody.Length >= sizeOfBitField)
        {
            Console.WriteLine("ОШИБКА: В ЭВМ размер числа ограничен разрядной сеткой процессора");
            Console.WriteLine($"Требуется: < {sizeOfBitField-1} бит для модуля, получено: {binaryStore.NumberBody.Length} бит");
            throw new ArgumentException($"The size of the bit field {sizeOfBitField} is too small to fully contain the binary representation of {strNum}");
        }
            
        var zerosToAdd = sizeOfBitField - 1 - binaryStore.NumberBody.Length;
        Console.WriteLine($"Шаг 2: Добавляем {zerosToAdd} ведущих нулей для выравнивания по разрядной сетке");
        
        var result = binaryStore.NumberSign.ToString() + binaryStore.NumberBody.Insert(0, new String('0', zerosToAdd));
        
        Console.WriteLine($"\nИТОГ: Прямой код числа {strNum} ({sizeOfBitField} бит):");
        Console.WriteLine($"Структура: [знак:{binaryStore.NumberSign}]|[модуль:{new String('0', zerosToAdd)}{binaryStore.NumberBody}]");
        Console.WriteLine($"Двоичное представление: {result}");
        
        return result;
    }
    
    // Конвертация десятичного числа в обратный код
    public static string ToOnesComplement(string strNum, int sizeOfBitField = 8)
    {
        Console.WriteLine("\n=== ОБРАТНЫЙ КОД (ONES COMPLEMENT) ===");
        Console.WriteLine("Теория: Обратный код упрощает вычитание через сложение");
        Console.WriteLine("Правила для отрицательных чисел:");
        Console.WriteLine("1. Получить двоичный модуль числа");
        Console.WriteLine("2. Инвертировать все биты модуля (0→1, 1→0)");
        Console.WriteLine("3. Установить знаковый бит = 1");
        
        var tempStore = ConvertToNumber(strNum);
        if (tempStore.NumberSign == 0)
        {
            Console.WriteLine("Число положительное: обратный код совпадает с прямым кодом");
            Console.WriteLine("Объяснение: Для положительных чисел все коды (прямой, обратный, дополнительный) идентичны");
            return ToSignedMagnitudeRepresentation(strNum);
        }

        var binaryStore = tempStore.FromDecimalToAnyBase(2).NumberBody;
        Console.WriteLine($"Шаг 1: Модуль числа в двоичной системе: {binaryStore}");

        if (binaryStore.Length < 7)
        {
            Console.WriteLine($"Шаг 2: Выравниваем до {sizeOfBitField-1} бит: {binaryStore}");
            binaryStore = binaryStore.Insert(0, new String('0', sizeOfBitField - 1 - binaryStore.Length));
        }
        
        Console.WriteLine("Шаг 3: Инвертируем все биты модуля");
        var invertedBinaryStore = new Number(1, ReverseBinary(binaryStore), 10);

        if (invertedBinaryStore.NumberBody.Length >= sizeOfBitField)
        {
            Console.WriteLine("ОШИБКА: Переполнение разрядной сетки процессора");
            throw new ArgumentException($"The size of the bit field {sizeOfBitField} is too small to fully contain the binary representation of {strNum}");
        }

        var result = "1" + invertedBinaryStore.NumberBody.Insert(0, new String('0', sizeOfBitField - 1 - invertedBinaryStore.NumberBody.Length));
        
        Console.WriteLine($"\nИТОГ: Обратный код числа {strNum} ({sizeOfBitField} бит):");
        Console.WriteLine("Особенность: В обратном коде два представления нуля: +0 (00000000) и -0 (11111111)");
        Console.WriteLine("Пример: -5 в обратном коде = инверсия (+5 = 00000101) = 11111010");
        Console.WriteLine($"Наш результат: {result}");
        
        return result;
    }

    // Конвертация десятичного числа в дополнительный код
    public static string ToTwosComplement(string strNum, int sizeOfBitField = 8)
    {
        Console.WriteLine("\n=== ДОПОЛНИТЕЛЬНЫЙ КОД (TWOS COMPLEMENT) ===");
        Console.WriteLine("Теория: Дополнительный код - современный стандарт представления целых чисел в ЭВМ");
        Console.WriteLine("Алгоритм для отрицательных чисел:");
        Console.WriteLine("1. Получить обратный код числа");
        Console.WriteLine("2. Прибавить 1 к полученному результату");
        
        var tempStore = ConvertToNumber(strNum);
        if (tempStore.NumberSign == 0)
        {
            Console.WriteLine("Число положительное: дополнительный код совпадает с прямым кодом");
            return ToSignedMagnitudeRepresentation(strNum);
        }
        
        Console.WriteLine("Шаг 1: Получаем обратный код числа");
        var tempBinaryStore = ToOnesComplement(strNum);
        
        Console.WriteLine($"Шаг 2: Прибавляем 1 к обратному коду ({tempBinaryStore} + 1)");
        var invertedBinaryStore = ConvertToNumber(tempBinaryStore);
        
        string newBody = Number.StringSum(invertedBinaryStore.NumberBody, "1", 2);
        Console.WriteLine($"Результат сложения: {newBody}");

        if (newBody.Length > sizeOfBitField - 1)
        {
            Console.WriteLine($"Произошло переполнение: игнорируем перенос за границы разрядной сетки");
            newBody = newBody.Substring(1);
        }

        if (newBody.Length >= sizeOfBitField)
        {
            Console.WriteLine($"ОШИБКА: Разрядная сетка процессора недостаточна для представления числа");
            throw new ArgumentException($"The size of the bit field {sizeOfBitField} is too small to fully contain the binary representation of {strNum}");
        }

        var result = "1" + newBody.Insert(0, new String('0', sizeOfBitField - 1 - newBody.Length));
        Console.WriteLine($"Наш результат: {result}");
        return result;
    }

    // Работа с вещественными числами
    // Используется формат IEEE 754 single
    public static string ToFloatRepresentation(string strNum, int sizeOfBitField = 32)
    {
        Console.WriteLine("\n=== ПРЕДСТАВЛЕНИЕ ВЕЩЕСТВЕННЫХ ЧИСЕЛ (IEEE 754 SINGLE PRECISION) ===");
        Console.WriteLine("Теория: Вещественные числа в ЭВМ представляются в формате с плавающей запятой");
        Console.WriteLine("Стандарт IEEE 754 (32 бита) использует:");
        Console.WriteLine("1 бит - знак (0=+, 1=-)");
        Console.WriteLine("8 бит - смещенный порядок (exponent bias = 127)");
        Console.WriteLine("23 бита - мантисса (значащие биты после запятой)");
        Console.WriteLine("Формат: (-1)^знак × 1.мантисса × 2^(порядок-127)");


        if (!strNum.Contains(".") && !strNum.Contains(","))
        {
            Console.WriteLine("ОШИБКА: Вещественные числа должны содержать разделитель целой и дробной части");
            throw new Exception($"The given number {strNum} does not contain a '.' or ',' character.");
        }
        int sizeOfSignBitField = 1;
        int sizeOfBiasBitField = 8;
        int sizeOfMantissaBitField = 23;
        var tempStore = ConvertToNumber(strNum);
        int signBit = tempStore.NumberSign;
        Console.WriteLine($"Знаковый бит: {signBit} (0=положительное, 1=отрицательное)");
        
        var binaryStr = tempStore.FromDecimalToAnyBase(2).NumberBody;
        Console.WriteLine($"Шаг 1: Двоичное представление: {binaryStr}");

        int normalizedShift = binaryStr.IndexOf('.') - 1;
        Console.WriteLine("Шаг 2: Нормализация - сдвигаем точку чтобы получить форму 1.xxxxx");
        Console.WriteLine($"Позиция точки: {binaryStr.IndexOf('.')}, нормализованный порядок: {normalizedShift}");
        
        var normalizedBinaryStr = binaryStr.Remove(binaryStr.IndexOf('.'), 1).Insert(1, ".");
        Console.WriteLine($"Нормализованная форма: {normalizedBinaryStr}");
        
        var mantissaStr = normalizedBinaryStr.Substring(2);
        Console.WriteLine($"Мантисса (биты после точки): {mantissaStr}");
        
        var biasedExponent = 127 + normalizedShift;
        Console.WriteLine($"Шаг 3: Вычисляем смещённый порядок: 127 + {normalizedShift} = {biasedExponent}");

        var binaryBiasedExponent = ConvertToNumber((biasedExponent).ToString()).FromDecimalToAnyBase(2).NumberBody;
        Console.WriteLine($"Смещенный порядок в двоичном виде: {binaryBiasedExponent}");

        if (binaryBiasedExponent.Length > sizeOfBiasBitField)
        {
            Console.WriteLine("ОШИБКА: Переполнение порядка - число слишком большое");
            throw new Exception($"The size of shifted order of the {strNum} is bigger than reserved 8 bit field");
        }
        

        if (mantissaStr.Length > sizeOfMantissaBitField)
        {
            Console.WriteLine("ОШИБКА: Точность ограничена 23 битами мантиссы");
            throw new Exception($"The size of mantissa without hidden bit of the number {strNum} is bigger than reserved 23 bit field.");
        }
        if (binaryBiasedExponent.Length < sizeOfBiasBitField)
        {
            binaryBiasedExponent = binaryBiasedExponent.Insert(0, new String('0', sizeOfBiasBitField - binaryBiasedExponent.Length));
            Console.WriteLine($"Порядок дополнен до 8 бит: {binaryBiasedExponent}");
        }

        if (mantissaStr.Length < sizeOfMantissaBitField)
        {
            mantissaStr = mantissaStr.Insert(mantissaStr.Length  , new String('0', sizeOfMantissaBitField - mantissaStr.Length));
            Console.WriteLine($"Мантисса дополнена до 23 бит: {mantissaStr}");
        }

        var result = signBit + binaryBiasedExponent + mantissaStr;
        
        Console.WriteLine($"\nИТОГ: IEEE 754 представление числа {strNum}: {result}");
        return result;
    }
}