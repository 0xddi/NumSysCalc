using System.Text;

namespace NumSysCalc;

public class CPURepresentation
{
    public static Number ConvertToNumber(string str)
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

    public static string ReverseBinary(string strNumber)
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
        var tempStore = ConvertToNumber(strNum);
        var binaryStore = tempStore.FromDecimalToAnyBase(2);
        if (binaryStore.NumberBody.Length >= sizeOfBitField)
            throw new ArgumentException($"The size of the bit field {sizeOfBitField} is too small to fully contain the binary representation of {strNum}");
        var result = binaryStore.NumberSign.ToString() + binaryStore.NumberBody.Insert(0, new String('0', sizeOfBitField - 1 - binaryStore.NumberBody.Length));
        return result;
    }
    
    // Конвертация десятичного числа в обратный код
    public static string ToOnesComplement(string strNum, int sizeOfBitField = 8)
    {
        var tempStore = ConvertToNumber(strNum);
        if (tempStore.NumberSign == 0) return ToSignedMagnitudeRepresentation(strNum);
        var binaryStore = tempStore.FromDecimalToAnyBase(2).NumberBody;
        if (binaryStore.Length < 7) binaryStore = binaryStore.Insert(0, new String('0', sizeOfBitField - 1 - binaryStore.Length));
        var invertedBinaryStore = new Number(1, ReverseBinary(binaryStore), 10);
        
        if (invertedBinaryStore.NumberBody.Length >= sizeOfBitField)
            throw new ArgumentException($"The size of the bit field {sizeOfBitField} is too small to fully contain the binary representation of {strNum}");
        var result = "1" + invertedBinaryStore.NumberBody.Insert(0, new String('0', sizeOfBitField - 1 - invertedBinaryStore.NumberBody.Length));
        return result;
    }

    // Конвертация десятичного числа в дополнительный код
    public static string ToTwosComplement(string strNum, int sizeOfBitField = 8)
    {
        var tempStore = ConvertToNumber(strNum);
        if (tempStore.NumberSign == 0) return ToSignedMagnitudeRepresentation(strNum);
        var tempBinaryStore = ToOnesComplement(strNum);
        var invertedBinaryStore = ConvertToNumber(tempBinaryStore);
        string newBody = Number.StringSum(invertedBinaryStore.NumberBody, "1", 2);
        if (newBody.Length > sizeOfBitField - 1) newBody = newBody.Substring(1);
        if (newBody.Length >= sizeOfBitField)
            throw new ArgumentException($"The size of the bit field {sizeOfBitField} is too small to fully contain the binary representation of {strNum}");
        var result = "1" + newBody.Insert(0, new String('0', sizeOfBitField - 1 - newBody.Length));
        return result;
    }

    // Работа с вещественными числами
    // Используется формат IEEE 754 single
    public static string ToFloatRepresentation(string strNum, int sizeOfBitField = 32)
    {
        if (!strNum.Contains(".") && !strNum.Contains(",")) throw new Exception($"The given number {strNum} does not contain a '.' or ',' character.");
        int sizeOfSignBitField = 1;
        int sizeOfBiasBitField = 8;
        int sizeOfMantissaBitField = 23;
        var tempStore = ConvertToNumber(strNum);
        int signBit = tempStore.NumberSign;
        var binaryStr = tempStore.FromDecimalToAnyBase(2).NumberBody;

        int normalizedShift = binaryStr.IndexOf('.') - 1;
        var normalizedBinaryStr = binaryStr.Remove(binaryStr.IndexOf('.'), 1).Insert(1, ".");
        var mantissaStr = normalizedBinaryStr.Substring(2);
        var binaryBiasedExponent = ConvertToNumber((127 + normalizedShift).ToString()).FromDecimalToAnyBase(2).NumberBody;
        if (binaryBiasedExponent.Length > sizeOfBiasBitField)
            throw new Exception($"The size of shifted order of the {strNum} is bigger than reserved 8 bit field");
        if (mantissaStr.Length > sizeOfMantissaBitField) throw new Exception($"The size of mantissa without hidden bit of the number {strNum} is bigger than reserved 23 bit field.");
        if (binaryBiasedExponent.Length < sizeOfBiasBitField)
        {
            binaryBiasedExponent = binaryBiasedExponent.Insert(0, new String('0', sizeOfBiasBitField - binaryBiasedExponent.Length));
        }

        if (mantissaStr.Length < sizeOfMantissaBitField)
        {
            mantissaStr = mantissaStr.Insert(mantissaStr.Length  , new String('0', sizeOfMantissaBitField - mantissaStr.Length));
        }
        
        return signBit + binaryBiasedExponent + mantissaStr;
    }
}