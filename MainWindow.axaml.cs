using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace OOP_Calculator;

public partial class MainWindow : Window
{
    private ISolve solver;
    string PreviousButton;
    public MainWindow() {
        InitializeComponent();
        solver = new Solver();
        TextBox1.Text = "";
    }
    // method for handling 0
    public bool isZero(double d) {
        if( d == 0 ) return true;
        return false;
    }
    public void ButtonHandler(object sender, RoutedEventArgs args) {
        if( sender is Button button ) {
            string ButtonText = button.Content.ToString();
            if(PreviousButton == "=")  {
                TextBox1.Text = "";
                PreviousButton = "";
            }

            switch (ButtonText) {
                // Numbers
                case ".":
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                case "0":
                    solver.Accumulate(ButtonText);
                    TextBox1.Text = string.Concat(TextBox1.Text, ButtonText);
                    break;
                //Special operators
                case "+/-":
                    string[] tokens = TextBox1.Text.Split(' ');
                    for(int i = tokens.Length - 1; i >= 0; i--) {
                        if(double.TryParse(tokens[i], out double lastNumber)) {
                            tokens[i] = (-lastNumber).ToString();
                            break;
                        }
                    }
                    TextBox1.Text = string.Join(" ", tokens);
                    solver.Clear();
                    solver.Accumulate(TextBox1.Text);
                    break;
                case "%":
                case "*":
                case "+":
                case "-":
                case "/":
                    TextBox1.Text = string.Concat(TextBox1.Text, " ", ButtonText, " ");
                    solver.Clear();
                    solver.Accumulate(TextBox1.Text);
                    break;
                case "=" :
                    PreviousButton = "=";
                    string postfixExpression = InfixToPostfix(TextBox1.Text);
                    double n = EvaluatePostfix(postfixExpression);
                    // rounds to the nearest 6 digits
                    if(double.IsNaN(n)) {
                        TextBox1.Text = "undefined";
                        break;
                    }
                    TextBox1.Text = Math.Round(n, 6).ToString();
                    break;
                case "AC":
                    TextBox1.Text = "";
                    break;
                default : 
                    TextBox1.Text = "Invalid Input";
                    break;
            }
        }
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
            if(char.IsDigit(ch) || (ch == '-' && wasPreviousOperator) || ch == '.') { 
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