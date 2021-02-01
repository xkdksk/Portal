
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UJ.Data
{
    public class ExpressionWrapper
    {


        public Expression expression;
        public HashSet<string> parameters;

        public Dictionary<string, double> argumentDic = new Dictionary<string, double>();
        public Dictionary<string, PropertiesData> propArgsDic = new Dictionary<string, PropertiesData>();

        public ExpressionWrapper()
        {
        }

        public ExpressionWrapper(Expression ex)
        {
            this.expression = ex;

            parameters = new HashSet<string>();
            foreach (var token in ex.postfix)
            {
                if (token.type == Token.Type.Argument)
                {
                    parameters.Add(token.name);
                }
            }


        }

        public void SetParam(string str, double val)
        {
            if (argumentDic.ContainsKey(str))
            {
                argumentDic[str] = val;
            }
            else
            {
                argumentDic.Add(str, val);
            }

        }

        public double GetParam(string str)
        {
            if (argumentDic.ContainsKey(str))
            {
                return argumentDic[str];
            }

            return 0;
        }




        public void SetPropAsParam(string str, PropertiesData prop)
        {
            if (propArgsDic.ContainsKey(str))
            {
                propArgsDic[str] = prop;
            }
            else
            {
                propArgsDic.Add(str, prop);
            }


        }



        public bool EvaluateBool()
        {
            try
            {
                return Convert.ToBoolean(expression.Calculate(argumentDic, propArgsDic));
            }
            catch (Exception e)
            {
                Debug.Log("EvaluateBool " + e.Message);
            }
            return false;
        }
        public double Evaluate()
        {

            return expression.Calculate(argumentDic, propArgsDic);


        }


        public double Evaluate(PropertiesData prop)
        {
            foreach (var e in parameters)
            {
                SetParam(e, prop.Get(e));

            }

            return Evaluate();
        }


        public double Evaluate(PropertiesData prop, IDictionary<string, double> valDic)
        {
            foreach (var e in parameters)
            {
                if (valDic.ContainsKey(e))
                {

                    SetParam(e, valDic[e]);
                }
                else
                {
                    SetParam(e, prop.Get(e));
                }
            }

            return Evaluate();
        }



        public double Evaluate(IDictionary<string, double> valDic)
        {
            foreach (var e in parameters)
            {

                if (valDic.ContainsKey(e))
                {

                    SetParam(e, valDic[e]);
                }

            }

            return Evaluate();
        }


        public double Evaluate(IDictionary<string, float> valDic)
        {
            foreach (var e in parameters)
            {

                if (valDic.ContainsKey(e))
                {

                    SetParam(e, valDic[e]);
                }

            }

            return Evaluate();
        }

        internal double EvaluateBySum(IDictionary<string, double> valDic, PropertiesData extraProps)
        {
            foreach (var e in parameters)
            {


                SetParam(e, valDic.GetValOrDefault(e) + extraProps.Get(e));

            }

            
            
            
            return Evaluate();
        }

        internal double Evaluate(PropertiesData propertiesData, Dictionary<string, double> pd)
        {
            foreach (var e in parameters)
            {
                if (pd.ContainsKey(e))
                {

                    SetParam(e, pd[e]);
                }
                else
                {
                    SetParam(e, propertiesData.Get(e));
                }
            }

            return Evaluate();
        }
    }


    public class HasFormula
    {
        public bool isParam;

        public HashSet<string> AllPropertyName = new HashSet<string>();

        Dictionary<string, HashSet<string>> propertyChain = new Dictionary<string, HashSet<string>>();

        public void AddPropertyChain(string changeOne, string applyOne)
        {

            if (propertyChain.ContainsKey(changeOne) == false)
            {
                propertyChain.Add(changeOne, new HashSet<string>());
            }


            if (propertyChain[changeOne].Contains(applyOne) == false)
            {
                propertyChain[changeOne].Add(applyOne);
            }


        }

        public Dictionary<string, HashSet<string>>.KeyCollection GetPropertyChainKeys()
        {
            return propertyChain.Keys;
        }

        public HashSet<string> GetPropertyChain(string property)
        {
            if (propertyChain.ContainsKey(property))
            {
                return propertyChain[property];
            }
            return null;

        }



        internal Dictionary<string, ExpressionWrapper> expressions = new Dictionary<string, ExpressionWrapper>();

        public IEnumerable<string> Symboms()
        {
            return expressions.Keys;
        }

        internal bool ContainsSymbol(string symbol)
        {
            return expressions.ContainsKey(symbol);
        }

        public double GetVal(string symbol)
        {
            var ex = GetExpression(symbol);
            if (ex == null)
            {
                return 0;
            }

            return ex.Evaluate();

        }


        public ExpressionWrapper GetExpression(string symbol)
        {

            if (expressions.ContainsKey(symbol))
            {

                return expressions[symbol];
            }

            Debug.LogError(symbol + "을 찾지 못했습니다.");
            return null;
        }



        protected void AddExpressionByFormula(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula))
                return;

            var firstEqualIdx = formula.IndexOf('=');

            var resultSymbol = formula.Substring(0, firstEqualIdx).Trim();
            var expressionStr = formula.Substring(firstEqualIdx + 1).Trim();

            var ex = new Expression(expressionStr);

            HashSet<string> parameters = new HashSet<string>();
            foreach (var token in ex.postfix)
            {

                if (token.type == Token.Type.Argument)
                {
                    parameters.Add(token.name);

                    AddPropertyChain(token.name, resultSymbol);
                    AllPropertyName.Add(token.name);
                }
            }
            AllPropertyName.Add(resultSymbol);

            var newEx = new ExpressionWrapper()
            {
                expression = ex,
                parameters = parameters

            };


            if (expressions.ContainsKey(resultSymbol))
            {
                expressions[resultSymbol] = newEx;
            }
            else
            {

                expressions.Add(resultSymbol, newEx);
            }


        }



    }
}