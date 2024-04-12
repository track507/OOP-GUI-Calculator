using System;
using System.Collections.Generic;
using System.Text;

public class Solver : ISolve
{
    private string accumulatedOperation = "";

    private bool isZero(double d) {
        if( d == 0 ) return true;
        return false;
    }

    public Solver()
    {
        Clear();
    }

    public void Accumulate(string s)
    {
        accumulatedOperation += s;
    }

    public void Clear()
    {
        accumulatedOperation = "";
    }

    public double Solve()
    {
        // Convert the accumulated operation to postfix notation
        string postfixExpression = InfixToPostfix(accumulatedOperation);

        // Evaluate the postfix expression
        double result = EvaluatePostfix(postfixExpression);
        if(double.IsNaN(result)) {
            throw new DivideByZeroException("Division by zero is not allowed.");
        }

        // Clear the accumulated operation for future calculations
        Clear();

        return Math.Round(result, 6);
    }
    // shunting-yard algorithm to determine PEMDAS order
    private string InfixToPostfix(string infixExpression) { 
        //debug statement
        Console.WriteLine("Infix expression: " + infixExpression);

        //our stack to keep track of operator order
        Stack<char> operatorStack = new Stack<char>();
        // our stringbuilder to keep track of our postfix expression and current numbers
        // we need to separate each token by a space so that we correctly use postfix notation
        StringBuilder postfix = new StringBuilder();
        // this keeps track of our current number whether its a multidigit or negative
        StringBuilder currentnumber = new StringBuilder();
        // "look up" to determine postfix order and precedence
        Dictionary<char, int> precedence = new Dictionary<char, int>() {
            {'+', 1},
            {'-', 1},
            {'*', 2},
            {'/', 2},
            {'%', 2}
        };

        bool wasPreviousOperator = true;

        foreach(char ch in infixExpression) {
            // if the current char is a digit, append it
            if(char.IsDigit(ch) || (ch == '-' && wasPreviousOperator)) { 
                currentnumber.Append(ch);
                wasPreviousOperator = false;
            }
            else {
                // if it is not a digit i.e. a white space or "+" etc... and currentnumber is not empty, we also append
                if(currentnumber.Length > 0) {
                    postfix.Append(currentnumber.ToString()).Append(" ");
                    currentnumber.Clear();
                }
                // it doesnt matter whether or not it has parenthesis, the infix will automatically determine the order
                if(ch == '(') operatorStack.Push(ch);
                else if(ch == ')') {
                    // we need to pop out all the operators until we find the opening parenthesis or is null i.e. (3 + 4) 
                    while(operatorStack.Peek() != '(') postfix.Append(operatorStack.Pop()).Append(" ");
                    operatorStack.Pop();
                }
                // now we know that it is neither a digit nor a parenthesis, so it must be an operator. 
                else if(precedence.ContainsKey(ch)) {
                    while(operatorStack.Count > 0 && precedence[ch] <= precedence[operatorStack.Peek()]) {
                        postfix.Append(operatorStack.Pop()).Append(" ");
                    }
                    operatorStack.Push(ch);
                    wasPreviousOperator = true;
                }
            }
        }
        Console.WriteLine("Postfix expression: " + postfix.ToString());
        // if there is still a current number left, we need to append it to our postfix
        if(currentnumber.Length > 0) postfix.Append(currentnumber.ToString()).Append(" ");
        // we now pop out all our operators
        while (operatorStack.Count > 0) {
            Console.WriteLine("Operator: " + operatorStack.Peek());
            postfix.Append(operatorStack.Pop()).Append(" ");
        }
        // remove any leading or trailing white spaces
        Console.WriteLine("Updated Postfix expression: " + postfix.ToString());
        return postfix.ToString().Trim();
    }

    private double EvaluatePostfix(string postfixExpression) {
        // this will hold our two operands, until we can find an operator.
        Stack<double> stack = new Stack<double>();
        //debug
        Console.WriteLine("Postfix Expression: " + postfixExpression);
        // loop through each ch and try to convert it to double. if it cannot convert, this means that we found an operator.
        foreach(string ch in postfixExpression.Split(' ')) {
            Console.WriteLine("current ch: " + ch);
            if(double.TryParse(ch, out double value)) stack.Push(value);
            else {
                //pop in "reverse" order since this is a stack
                double operand2 = stack.Pop();
                Console.WriteLine("Operand2: " +operand2);
                double operand1 = stack.Pop();
                Console.WriteLine("Operand1: " + operand1);
                
                //debug
                Console.WriteLine(operand1 + " " + operand2);
                switch(ch) {
                    case "%":
                        stack.Push(operand1 % operand2);
                        break;
                    case "+":
                        stack.Push(operand1 + operand2);
                        break;
                    case "-":
                        stack.Push(operand1 - operand2);
                        break;
                    case "*":
                        stack.Push(operand1 * operand2);
                        break;
                    case "/":
                        // must check if it is zero, if it is, return undefined.
                        if( isZero(operand2) ) return double.NaN;
                        stack.Push(operand1 / operand2);
                        break;
                }
            }
        }
        return stack.Pop();
    }
}