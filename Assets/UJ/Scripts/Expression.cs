

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace UJ.Data
{
    public struct Token
    {

        public enum Type
        {
            Operator,
            Argument,
            Number,
            BracketStart,
            BracketEnd,
            Function,
            Comma
        }
        public Type type;
        public string name;
        public string[] nameSplit;

        public enum Operator
        {
            None,
            Add,
            Minus,
            Multiply,
            Divide,
            Power,
            Random,
            Equal,
            Over,
            Less,
            Left,
            OverEqual,
            LessEqual,
            NotEqual,
        }
        public double v;
        public Operator op;

        public override string ToString()
        {
            switch (type)
            {
                case Type.Argument:
                    return name;
                case Type.BracketEnd:
                    return "(";
                case Type.BracketStart:
                    return ")";
                case Type.Number:
                    return v.ToString();
                case Type.Operator:
                    return op.ToString();
            }
            return "";

        }

        internal double Calulate(Token a, Token b)
        {


            switch (op)
            {
                case Operator.Left:
                    return a.v % b.v;

                case Operator.Add:
                    return a.v + b.v;
                case Operator.Divide:
                    return a.v / b.v;
                case Operator.Minus:
                    return a.v - b.v;
                case Operator.Multiply:
                    return a.v * b.v;
                case Operator.Power:
                    return Math.Pow(a.v, b.v);
                case Operator.Equal:
                    return Math.Abs(a.v - b.v) < 0.0001f ? 1 : 0;
                case Operator.OverEqual:
                    return (Math.Abs(a.v - b.v) < 0.0001f || a.v > b.v) ? 1 : 0;
                case Operator.LessEqual:
                    return (Math.Abs(a.v - b.v) < 0.0001f || a.v < b.v) ? 1 : 0;
                case Operator.NotEqual:
                    return Math.Abs(a.v - b.v) < 0.0001f ? 0 : 1;
                case Operator.Over:
                    return a.v > b.v ? 1 : 0;
                case Operator.Less:
                    return a.v < b.v ? 1 : 0;
                case Operator.Random:
                    return UnityEngine.Random.Range((float)a.v, (float)b.v);

            }
            return 0;


        }


        static int precedence(Token t)
        {
            if (t.type == Token.Type.BracketStart) return 0;
            if (t.IsCompareOperator()) return 1;
            if (t.op == Token.Operator.Add || t.op == Token.Operator.Minus) return 2;
            if (t.op == Token.Operator.Multiply || t.op == Token.Operator.Divide || t.op == Operator.Random || t.op == Operator.Left) return 3;
            if (t.op == Token.Operator.Power) return 4;

            else return 4;
        }


        public static List<Token> ToPostfixNotation(List<Token> tokens)
        {
            Stack<Token> stack = new Stack<Token>();
            List<Token> postfix = new List<Token>();


            foreach (var t in tokens)
            {
                if (t.type == Token.Type.BracketStart)
                {
                    stack.Push(t);
                    continue;
                }
                if (t.type == Type.Comma)
                {
                    Token s;

                    while (stack.Count != 0 && stack.Peek().type != Token.Type.BracketStart)
                    {
                        s = stack.Pop();
                        if (s.type == Type.Comma)
                        {
                            continue;
                        }
                        postfix.Add(s);

                    }


                    stack.Push(t);
                    continue;
                }

                if (t.type == Token.Type.BracketEnd)
                {
                    Token s;

                    while (stack.Count != 0 && stack.Peek().type != Token.Type.BracketStart)
                    {
                        s = stack.Pop();

                        if (s.type == Type.Comma)
                        {
                            continue;
                        }
                        postfix.Add(s);

                    }
                    s = stack.Pop();
                    if (stack.Count > 0 && stack.Peek().type == Type.Function)
                    {
                        s = stack.Pop();
                        postfix.Add(s);
                    }

                    continue;
                }

                if (t.type == Token.Type.Function)
                {

                    stack.Push(t);
                    continue;
                }
                if (t.type == Token.Type.Operator)
                {
                    Token s;
                    while (stack.Count != 0 && precedence(stack.Peek()) >= precedence(t))
                    {
                        s = stack.Pop();
                        if (s.type == Type.Comma)
                        {
                            continue;
                        }
                        postfix.Add(s);

                    }
                    stack.Push(t);
                    continue;
                }

                if (t.type == Token.Type.Argument || t.type == Token.Type.Number)
                {
                    postfix.Add(t);

                    continue;
                }
            }

            while (stack.Count != 0)
            {
                var t = stack.Pop();

                postfix.Add(t);
            }

            return postfix;

        }

        public static List<Token> Tokenize(string str)
        {
            int i;
            var len = str.Length;

            List<Token> tokens = new List<Token>();

            for (i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (char.IsWhiteSpace(c))
                    continue;



                var op = GetOperator(c);

                if ((tokens.Count == 0 || tokens.Last().IsCompareOperator()) && op == Operator.Minus)
                {
                    tokens.Add(new Token()
                    {
                        type = Token.Type.Number,
                        v = -1
                    });
                    tokens.Add(new Token()
                    {
                        type = Token.Type.Operator,
                        op = Operator.Multiply
                    });


                }
                else
                {
                    if (op == Operator.None)
                    {
                        if (c == '!' && str[i + 1] == '=')
                        {
                            op = Operator.NotEqual;
                            i++;
                            tokens.Add(new Token()
                            {
                                type = Token.Type.Operator,
                                op = op
                            });
                            continue;
                        }

                    }

                    if (op != Token.Operator.None)
                    {
                        if (op == Operator.Less)
                        {
                            if (str[i + 1] == '=')
                            {
                                i++;
                                op = Operator.LessEqual;
                            }
                        }
                        else if (op == Operator.Over)
                        {
                            if (str[i + 1] == '=')
                            {
                                i++;
                                op = Operator.OverEqual;
                            }
                        }
                        else if (op == Operator.Equal)
                        {
                            if (str[i + 1] != '=')
                            {
                                throw new Exception("Wrong Expression");
                            }
                            i++;

                        }

                        tokens.Add(new Token()
                        {
                            type = Token.Type.Operator,
                            op = op
                        });
                        continue;
                    }
                }

                if (char.IsDigit(c))
                {
                    tokens.Add(getNumberToken(str, ref i));

                    continue;
                }



                if (char.IsLetter(c))
                {
                    tokens.Add(getArgumentToken(str, ref i));

                    continue;
                }
                if (c == ',')
                {
                    tokens.Add(new Token()
                    {
                        type = Type.Comma
                    });
                }

                if (c == '(')
                {
                    if (tokens.Count > 0)
                    {
                        var preToken = tokens.Last();
                        if (preToken.type == Type.Argument)
                        {
                            preToken.type = Type.Function;
                            tokens[tokens.Count - 1] = preToken;

                        }
                    }

                    tokens.Add(new Token()
                    {
                        type = Token.Type.BracketStart
                    });
                    continue;
                }

                if (c == ')')
                {
                    tokens.Add(new Token()
                    {
                        type = Token.Type.BracketEnd
                    });
                    continue;
                }
            }

            return tokens;
        }

        private bool IsCompareOperator()
        {

            return op == Operator.Less || op == Operator.Equal || op == Operator.Over || op == Operator.NotEqual ||
                op == Operator.LessEqual || op == Operator.OverEqual;
        }

        private static Token getNumberToken(string str, ref int i)
        {
            var startIndex = i;

            while (i < str.Length)
            {
                var c = str[i];
                if (!(char.IsDigit(c) || c == '.'))
                {
                    break;
                }
                i++;
            }
            i--;
            var v = ParseDouble(str, startIndex, i);

            return new Token()
            {
                type = Token.Type.Number,
                v = v
            };

        }

        private static Token.Operator GetOperator(char c)
        {

            switch (c)
            {
                case '*':
                    return Token.Operator.Multiply;
                case '+':
                    return Token.Operator.Add;
                case '-':
                    return Token.Operator.Minus;
                case '^':
                    return Token.Operator.Power;
                case '/':
                    return Token.Operator.Divide;
                case '%':
                    return Operator.Left;
                case '<':
                    return Token.Operator.Less;
                case '>':
                    return Token.Operator.Over;
                case '=':
                    return Token.Operator.Equal;
                case '~':
                    return Token.Operator.Random;

            }

            return Token.Operator.None;
        }


        private static Token getArgumentToken(string str, ref int i)
        {
            var startIndex = i;

            while (i < str.Length)
            {
                var c = str[i];
                if (!(char.IsLetterOrDigit(c) || c == '_' || c == '.'))
                {
                    break;
                }
                i++;
            }
            var nameStr = str.Substring(startIndex, i - startIndex);
            i--;



            return new Token()
            {
                type = Token.Type.Argument,
                name = nameStr,
                nameSplit = nameStr.Split('.')
            };


        }

        private static double ParseDouble(string str, int startIdx, int endIdx)
        {
            int i = startIdx;
            int sign = 1;
            double v = 0;
            int underIdx = endIdx;
            var c = str[i];
            if (c == '-')
            {
                sign = -1;
                i++;
            }
            for (; i <= endIdx; i++)
            {
                c = str[i];
                if (c == '.')
                {
                    underIdx = i;
                    continue;
                }
                c -= '0';
                v *= 10;
                v += c;
            }

            v *= sign;
            v *= Math.Pow(0.1, endIdx - underIdx);
            return v;
        }

    }

    public class Expression
    {
        public List<Token> postfix;

        string orignalStr;

        public string GetStr()
        {
            return orignalStr;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(orignalStr);
        }

        public Expression(string str)
        {
            if (str == null)
            {
                str = "";
            }
            orignalStr = str;


            var tokens = Token.Tokenize(str);
            postfix = Token.ToPostfixNotation(tokens);
        }


        Stack<Token> stack = new Stack<Token>();




        public double Calculate(IDictionary<string, double> args, IDictionary<string, PropertiesData> propArg)
        {
            stack.Clear();

            foreach (var t in postfix)
            {
                if (t.type == Token.Type.Argument || t.type == Token.Type.Number)
                {
                    stack.Push(t);
                    continue;
                }
                if (t.type == Token.Type.Function)
                {
                    //인자 개수만큼 Pop
                    //토큰을 저장

                    switch (t.name)
                    {
                        case "RoundUp":
                            {
                                var digits = stack.Pop();
                                var number = stack.Pop();

                                FillTokenVal(args, propArg, ref digits);
                                FillTokenVal(args, propArg, ref number);

                                var factor = Math.Pow(10, digits.v);


                                stack.Push(new Token()
                                {
                                    type = Token.Type.Number,
                                    v = Math.Ceiling(number.v * factor) / factor
                                });

                            }
                            break;

                        case "Max":
                            {
                                var b = stack.Pop();
                                var a = stack.Pop();

                                FillTokenVal(args, propArg, ref b);
                                FillTokenVal(args, propArg, ref a);

                                stack.Push(new Token()
                                {
                                    type = Token.Type.Number,
                                    v = a.v > b.v ? a.v : b.v
                                });
                            }
                            break;
                        case "Min":
                            {
                                var b = stack.Pop();
                                var a = stack.Pop();

                                FillTokenVal(args, propArg, ref b);
                                FillTokenVal(args, propArg, ref a);

                                stack.Push(new Token()
                                {
                                    type = Token.Type.Number,
                                    v = a.v < b.v ? a.v : b.v
                                });
                            }
                            break;
                    }


                }

                if (t.type == Token.Type.Operator)
                {
                    var b = stack.Pop();
                    var a = stack.Pop();

                    FillTokenVal(args, propArg, ref b);
                    FillTokenVal(args, propArg, ref a);
                    stack.Push(new Token()
                    {
                        type = Token.Type.Number,
                        v = t.Calulate(a, b)
                    });
                }
            }

            try
            {
                var lastToken = stack.Pop();

                if (lastToken.type == Token.Type.Argument)
                {


                    FillTokenVal(args, propArg, ref lastToken);
                    return lastToken.v;
                }

                return lastToken.v;
            }
            catch (Exception e)
            {
                Debug.LogError("ExpressionError " + orignalStr);
                return 0;
            }
        }


        private static void FillTokenVal(IDictionary<string, double> args, IDictionary<string, PropertiesData> propArg, ref Token b)
        {

            if (b.type == Token.Type.Argument)
            {

                if (b.nameSplit.Length > 1)
                {
                    var pName = b.nameSplit[0];
                    if (propArg.ContainsKey(pName))
                    {
                        var pd = propArg[pName];
                        b.v = pd.Get(b.nameSplit[1]);
                    }
                    else
                    {
                        b.v = 0;
                    }
                }
                else if (args.ContainsKey(b.name))
                {
                    b.v = args[b.name];
                }
                else
                {
                    b.v = 0;
                }

            }


        }


    }
}