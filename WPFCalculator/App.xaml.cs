using System.Configuration;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System;

namespace WPFCalculator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

     
    }      

}

class Operations<T>
{
    // i wanted to make the calculator work with every type of variables (float, double, decimal) for starters but it would make everything more complicated and my knowledge is not good enaugh yet in C#
    // Ignore the comment above, I made it work with decimals so everything should be good, it was the best decision to use template
    private Dictionary<char, Func<T, T, T>> operations = new Dictionary<char, Func<T, T, T>>
    {
        { '-', (x, y) => (dynamic?)x - (dynamic?)y }, // had to add ? in front of dynaic since got a warning for null
        { '+', (x, y) => (dynamic?)x + (dynamic?)y },
        { '*', (x, y) => (dynamic?)x * (dynamic?)y },
        { 'x', (x, y) => (dynamic?)x * (dynamic?)y },
        { '/', (x, y) => (dynamic?)x / (dynamic?)y },
        { ':', (x, y) => (dynamic?)x / (dynamic?)y },
    };

    public T Calculate(T x, T y, char recievedOperator)
    {
        if (operations.ContainsKey(recievedOperator))
        {
            // ahh i hate myself for making this func use templates || handle devidion by 0 so we doesnt crash hehe :D
            if ((recievedOperator == '/' || recievedOperator == ':') && (dynamic?)y == 0)
                throw new InvalidDataException("Can not devide with 0!!!");

            return operations[recievedOperator](x, y);
        }
        throw new InvalidDataException("Passed operator is invalid! Only the four default operator is supported.");
    }
}

class CalculatorClass
{

    public bool checkMathematicalStringValidity(string data)
    {
        //operators we allow
        char[] operators = { '-', '+', '*', 'x', ':', '/' };

        // ik i could save on variables but its more readable this way
        bool isStringInvalid = false;
        bool isContainAnyOperator = false;
        bool charactersCorrect = false;
        bool operatorsNextToEachOther = false;

        charactersCorrect = Regex.IsMatch(data, @"^[0-9]+(?:\.[0-9]+)?(?:\s*[+\-*/:x]\s*[0-9]+(?:\.[0-9]+)?)*$"); // regex to check if inputed string only contain valid data (filters out characters that is not mathematical operators or a dot)
        if (!charactersCorrect)
            Console.WriteLine("Please Input only allowed characters! Allowed chars: +-*/:. and numbers");
        //check if there are multiple operators next to each other
        for (int item = 0; item < operators.Length; item++)
        {
            int buffer = data.IndexOf(operators[item]);
            if (buffer < 0) // operator not found
                continue;

            isContainAnyOperator = true; // no additional checks since if we reach this part it means we already have an operator present since IndexOf passed

            if (operators.Contains(data[buffer + 1]))
            {
                Console.WriteLine("Two or more operators cant be next to each other!");
                operatorsNextToEachOther = true;
            }

            //throw new InvalidDataException("Two or more operators cant be next to each other!");
        }

        isStringInvalid = operators.Any(option => data.StartsWith(option)) || operators.Any(option => data.EndsWith(option));
        if (isStringInvalid)
            Console.WriteLine("Invalid inputed data, string cant start/end with an operator!");

        return ((operatorsNextToEachOther || isStringInvalid) || !charactersCorrect || !isContainAnyOperator);
    }

    public decimal SplitAtOperator(string input)
    {
        //char[] delimiterChars = { '-', '+', '*', ':', '/' };
        char[] weakOperators = { '-', '+' };
        char[] strongOperators = { '*', 'x', ':', '/' };

        // create a lambda function to check delimiter positions. Note: I know a function within the class would be more readable but I wanted to learn how lambdas work in C#
        var ProcessOperators = (string inputString, char[] delimiters, bool strong = false) =>
        {
            int index = -1;
            for (int item = 0; item < delimiters.Length; item++)
            {
                // get the index of our operator
                int buffer = strong ? inputString.LastIndexOf(delimiters[item]) : inputString.IndexOf(delimiters[item]);
                if (buffer < 0)
                    continue;


                if (strong)
                {
                    if (buffer > index)
                        index = buffer;
                }
                else
                    index = buffer;


            }
            return index;
        };

        int strongDelimitersPos = ProcessOperators(input, strongOperators, true);
        int weakDelimitersPos = ProcessOperators(input, weakOperators);
        if (strongDelimitersPos <= 0 && weakDelimitersPos <= 0)
            throw new Exception("Could not find any mathematical operator"); // we have a problem here


        int index = weakDelimitersPos < 0 ? strongDelimitersPos : weakDelimitersPos; // determinate what to work with


        char stringOperator = input[index];
        string part1 = input.Substring(0, index);
        string part2 = input.Substring(index + 1);

        return calc(part1, part2, stringOperator);

    }

    //calculation function
    private decimal calc(string X, string Y, char Operator)
    {
        decimal xFinalValue = 0;
        decimal yFinalValue = 0;
        if (!Decimal.TryParse(X, out decimal numValueX)) // Trying to convert string to decimal value if we fail we split again since our string is not a valu!
        {
            xFinalValue = SplitAtOperator(X);
        }
        else
            xFinalValue = numValueX;

        if (!Decimal.TryParse(Y, out decimal numValueY))
        {
            yFinalValue = SplitAtOperator(Y);
        }
        else
            yFinalValue = numValueY;


        var MathCalculator = new Operations<decimal>();
        var returnVal = MathCalculator.Calculate(xFinalValue, yFinalValue, Operator); // variables will always be filled sine we continue spliting our string until we can solve a math prob

#if DEBUG
        Console.WriteLine($"({X}) {Operator} ({Y}) = {returnVal}");
#endif

        return returnVal;
    }



}
