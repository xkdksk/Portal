using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine;


namespace UJ.Data
{




    public interface IHasPropertiesData
    {
        PropertiesData GetPropertiesData();
    }

    public class PropertiesData 
    {
        public ChangeNotifier notifier = new ChangeNotifier();

    

        internal void ClearCache()
        {

            properties.Clear();
        }

        public void Incr(string key, double val)
        {

            ChainSet(key, Get(key) + val);
        }



        public Dictionary<string, double> properties = new Dictionary<string, double>();
        //  public Expression


        public Dictionary<string, Dictionary<int, double>> LayerValues = new Dictionary<string, Dictionary<int, double>>();

        public Dictionary<int, Dictionary<string, PropertiesData>> propParamContainer = new Dictionary<int, Dictionary<string, PropertiesData>>();

        public Dictionary<int, Dictionary<string, double>> paramContainer = new Dictionary<int, Dictionary<string, double>>();
        bool paramZeroSetted = false;


        Dictionary<string, PropertiesData> propContainer = new Dictionary<string, PropertiesData>();

        public void SetPropConnect(string key, PropertiesData props)
        {
            if (propContainer.ContainsKey(key) == false)
            {
                propContainer.Add(key, props);
            }
            else
            {
                if (propContainer[key] == props)
                {
                    return;
                }

                propContainer[key].subscProperties.Remove(this);
            }

            propContainer[key] = props;

            if (props.subscProperties.Contains(this) == false)
            {
                props.subscProperties.Add(this);
            }

        }



        public void SetPropAsParam(int paramKindCode, string key, PropertiesData props)
        {
            if (paramKindCode == 0)
            {
                throw new System.Exception("Wrong Param! paramKindCode Zero means not using it");
            }
            if (propParamContainer.ContainsKey(paramKindCode) == false)
            {
                propParamContainer[paramKindCode] = new Dictionary<string, PropertiesData>();
            }

            propParamContainer[paramKindCode][key] = props;

        }

        //파라미터 합이되지 않는다.
        //해당 paramKindCode에서만 공유된다.
        public void SetParam(int paramKindCode, string key, double value, bool applyChainSet = true)
        {
            if (paramKindCode == 0)
            {
                throw new System.Exception("Wrong Param paramKindCode Zero means not using it");
            }

            if (paramContainer.ContainsKey(paramKindCode) == false)
            {
                paramContainer[paramKindCode] = new Dictionary<string, double>();
            }

            paramContainer[paramKindCode][key] = value;

            if (applyChainSet)
            {
                if (propInfos.ContainsKey(key))
                {
                    var propInfo = propInfos[key];
                    foreach (var p in propInfo.chainProps)
                    {
                        UpdateProperty(p);
                    }
                }


            }
        }


        public class PropInfo
        {
            //      public Dictionary<ExpressionWrapper,int> expressions = new Dictionary<ExpressionWrapper,int>();

            public Dictionary<int, HashSet<ExpressionWrapper>> expressions = new Dictionary<int, HashSet<ExpressionWrapper>>();

            public HashSet<string> chainProps = new HashSet<string>();



            public Func<double, double, double> calFunc;

            public PropInfo Clone()
            {
                Dictionary<int, HashSet<ExpressionWrapper>> expressionsClone = new Dictionary<int, HashSet<ExpressionWrapper>>();
                HashSet<string> chainPropsClone = new HashSet<string>();

                var iter= expressions.GetEnumerator();

                while(iter.MoveNext())
                {
                    var e = iter.Current;

                    var hs = e.Value;

                    var newHs = new HashSet<ExpressionWrapper>();
                    foreach (var ex in hs)
                    {
                        newHs.Add(ex);
                    }
                    expressionsClone.Add(e.Key, newHs);

                }


                foreach (var ex in chainProps)
                {
                    chainPropsClone.Add(ex);
                }

                return new PropInfo()
                {
                    expressions = expressionsClone,
                    chainProps = chainPropsClone,
                    calFunc = calFunc
                };
            }



            public void AddPassive(string key, HasFormula passive, int paramKindCode)
            {

                var propChain = passive.GetPropertyChain(key);
                if (propChain != null)
                {
                    foreach (var k in propChain)
                    {
                        chainProps.Add(k);
                    }
                }

                if (passive.ContainsSymbol(key))
                {
                    var expression = passive.GetExpression(key);

                    if (expressions.ContainsKey(paramKindCode) == false)
                    {
                        expressions.Add(paramKindCode, new HashSet<ExpressionWrapper>());
                    }

                    if (expressions[paramKindCode].Contains(expression) == false)
                    {
                        expressions[paramKindCode].Add(expression);
                    }
                }
            }


            public void Remove(int paramKindCode)
            {

                expressions.Remove(paramKindCode);

            }



        }

        internal void CopyFrom(PropertiesData prop)
        {
            Clear();


            int i;

            var iter= prop.properties.GetEnumerator();

            
            while(iter.MoveNext())
            {
                var kv = iter.Current;

                this.Set(kv.Key, prop.Get(kv.Key));
            }

        }

        public Dictionary<string, PropInfo> propInfos = new Dictionary<string, PropInfo>();

        public void Clear()
        {
            paramZeroSetted = false;
            propInfos.Clear();
            paramContainer.Clear();
            propParamContainer.Clear();
            properties.Clear();
            LayerValues.Clear();
        }

        public void SetCalFunc(string propName, Func<double, double, double> calFunc)
        {

            if (propInfos.ContainsKey(propName) == false)
            {
                propInfos.Add(propName, new PropInfo());
            }


            propInfos[propName].calFunc = calFunc;
            UpdateProperty(propName);

        }


        public PropertiesData Clone()
        {
            var propInfoClone = new Dictionary<string, PropInfo>();
            var paramContainerClone = new Dictionary<int, Dictionary<string, double>>();
            var propertiesClone = new Dictionary<string, double>(properties);

            var propContainerClone = new Dictionary<int, Dictionary<string, PropertiesData>>();

            int i;
            {
                var iter = propInfos.GetEnumerator();
                while (iter.MoveNext())
                {
                    var p = iter.Current;

                    propInfoClone.Add(p.Key, p.Value.Clone());
                }
            }


            {
                var iter = paramContainer.GetEnumerator();
                while (iter.MoveNext())
                {
                    var p = iter.Current;


                    paramContainerClone.Add(p.Key, new Dictionary<string, double>(p.Value));
                }
            }

            {
                var iter = propContainerClone.GetEnumerator();
                while (iter.MoveNext())
                {
                    var p = iter.Current;
            

                    propContainerClone.Add(p.Key, new Dictionary<string, PropertiesData>(p.Value));
                }
            }


            return new PropertiesData()
            {
                paramZeroSetted = this.paramZeroSetted,
                propInfos = propInfoClone,
                paramContainer = paramContainerClone,

                propParamContainer = propContainerClone,
                properties = propertiesClone

            };

        }

        public void FillPropInfo(HasFormula p, int paramKindCode)
        {

            foreach (var k in p.AllPropertyName)
            {
                if (propInfos.ContainsKey(k) == false)
                {
                    propInfos.Add(k, new PropInfo());
                }




                if (p.isParam)
                {
                    SetParam(paramKindCode, p);
                }
                else
                {

                    var dic = propInfos[k];
                    dic.AddPassive(k, p, paramKindCode);
                }


            }

        }




        public bool Has(string key)
        {
            return properties.ContainsKey(key);
        }



        public void Set(string key, double val)
        {
            if (properties.ContainsKey(key) == false)
            {
                properties.Add(key, val);
            }
            else
            {
                properties[key] = val;
            }


            notifier.Publish<double>(key, val);

            foreach (var s in subscProperties)
            {
                s.applyConnectedPropValueChange(this, key, val);
            }

        }




        private void applyConnectedPropValueChange(PropertiesData propertiesData, string key, double val)
        {
            int i;

            var iter= propContainer.GetEnumerator();

            while(iter.MoveNext())
            {
                var kv = iter.Current;
                var hostName = kv.Key;
                if (kv.Value != propertiesData)
                {
                    continue;
                }

                var propName = hostName + "." + key;
                bool found = false;


                var iter2= properties.GetEnumerator();

                while(iter2.MoveNext())
                {
                    var keyName = iter2.Current.Key;
                    if (keyName == propName)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    ChainSet(propName, val);
                }
            }
        }

        public List<PropertiesData> subscProperties = new List<PropertiesData>();


        public double Get(string key)
        {
            if (properties.ContainsKey(key) == false)
            {
                return 0; //기본값
            }

            return properties[key];
        }







        public void ClearAndApplyPassive(IEnumerable<HasFormula> passives, int paramKindCode)
        {
            Clear();

            ApplyPassive(passives, paramKindCode);

        }


        public void ApplyPassive(HasFormula formula, int paramKindCode)
        {
            FillPropInfo(formula, paramKindCode);

            foreach (var p in formula.AllPropertyName)
            {
       
                UpdateProperty(p);
            }
        }



        public void ApplyPassive(IEnumerable<HasFormula> passives, int paramKindCode, bool updateProperties = true, bool append = false)
        {



            if (append == false)
            {
                if (paramKindCode == 0)
                {
                    if (paramZeroSetted)
                    {
                        return;
                    }
                    paramZeroSetted = true;
                }
                else
                {
                    if (propInfos.Any(l => l.Value.expressions.Any(e => e.Key == paramKindCode)))
                    {
                        return;
                    }
                }
            }

            foreach (var p in passives)
            {
                if (p.isParam)
                {
                    SetParams(passives, paramKindCode);
                }
                else
                {
                    FillPropInfo(p, paramKindCode);
                }
            }

            if (updateProperties)
            {
                UpdateProperties(passives);
            }

            /*
            for (var k in propInfos)
            {
                //만족하면

                var pName = k.Key;
                UpdateProperty(pName);
            }
            */

        }

        private void SetParams(IEnumerable<HasFormula> passives, int paramKindCode)
        {
            foreach (var p in passives)
            {
                SetParam(paramKindCode, p);
            }

            //한번더 하면 제대로 순서에 상관없이 잘 될것이다.
            //ex)   a=b  ,b=30  이런것이 순서에 구애받지 않는다. 
            foreach (var p in passives)
            {
                SetParam(paramKindCode, p);
            }
        }

        private void SetParam(int paramKindCode, HasFormula p)
        {
            foreach (var n in p.Symboms())
            {
                var ex = p.GetExpression(n);


                if (paramContainer.ContainsKey(paramKindCode) == false)
                {
                    paramContainer[paramKindCode] = new Dictionary<string, double>();
                }


                var pd = paramContainer[paramKindCode];


                var v = ex.Evaluate(this, pd);

                pd[n] = v;



            }
        }



        static string[] tempStrArray = new string[5000];
        public void UpdateProperties(IEnumerable<HasFormula> passives)
        {

            int i = 0;
            int fillCnt = 0;

            foreach (var p in passives)
            {

                foreach (var n in p.AllPropertyName)
                {
                    for (i = 0; i < fillCnt; i++)
                    {
                        if (tempStrArray[i] == n)
                        {
                            break;
                        }

                    }
                    if (i == fillCnt)
                    {
                        tempStrArray[fillCnt] = n;
                        fillCnt++;
                    }
                }
            }

            for (i = 0; i < fillCnt; i++)
            {
                var n = tempStrArray[i];
                if (propInfos.ContainsKey(n) == false)
                {
                    continue;
                }
                UpdateProperty(n);
            }


        }

        public void UpdateProperties()
        {
            int i;

            var iter = propInfos.GetEnumerator();
            while(iter.MoveNext())
            {                    
                UpdateProperty(iter.Current.Key);
            }
        }

        public void RemovePassive(int paramCode)
        {
            HashSet<string> changedKeys = new HashSet<string>();
            var iter = propInfos.GetEnumerator();
            while (iter.MoveNext())
            {
                var pi = iter.Current;

                if (pi.Value.expressions.ContainsKey(paramCode))
                {
                    pi.Value.expressions.Remove(paramCode);
                    changedKeys.Add(pi.Key);
                }
            }

            foreach (var k in changedKeys)
            {
                UpdateProperty(k);
            }

        }

        public void SetLayerValue(int layer, string key, double val)
        {
            if (LayerValues.ContainsKey(key) == false)
            {
                LayerValues.Add(key, new Dictionary<int, double>());
            }


            LayerValues[key][layer] = val;

            UpdateProperty(key);

        }

        public IEnumerable<string> GetLayerPropNames(int layer)
        {

            return LayerValues.Where(l => l.Value.ContainsKey(layer)).Select(l => l.Key);

        }

        public bool HasLayer(int layer)
        {

            return LayerValues.Any(l => l.Value.ContainsKey(layer));

        }


        string[] tempArr = new string[100];
        public void RemoveLayer(int layer)
        {
            int victimIdx = 0;

            //    HashSet<string> vicitms = new HashSet<string>();


            var iter = LayerValues.GetEnumerator();
            while(iter.MoveNext())
            {
                var l = iter.Current;
                if (l.Value.ContainsKey(layer) == false)
                {
                    continue;
                }

                var v = l.Value[layer];
                ChainSet(l.Key, Get(l.Key) - v);

                if (l.Value.Remove(layer))
                {
                    int i;
                    bool has = false;
                    for (i = 0; i < victimIdx; i++)
                    {
                        if (tempArr[i] == l.Key)
                        {
                            has = true;
                            break;
                        }
                    }
                    if (has == false)
                    {
                        tempArr[victimIdx] = l.Key;
                        victimIdx++;
                    }


                }
            }


            {
                int i;
                for (i = 0; i < victimIdx; i++)
                {
                    var v = tempArr[i];
                    UpdateProperty(v);
                    //        Debug.Log("RemoveLayer" + layer+" AfterVal " + v + ": " + Get(v));
                }
            }

        }


        public void UpdateProperty(string pName)
        {



            double sum = 0;
            if (propInfos.ContainsKey(pName))
            {

                var propInfo = propInfos[pName];
                var expressions = propInfo.expressions;
                bool notEnoughParam = IsNotEnoughParam(expressions);
                if (notEnoughParam)
                {
                    return;
                }

                var iter= expressions.GetEnumerator();

                while(iter.MoveNext())
                {
                    var s = iter.Current;

                    var paramKindCode = s.Key;
                    foreach (var ex in s.Value)
                    {
                        double v = 0;


                        if (paramKindCode != 0)
                        {
                            if (propParamContainer.ContainsKey(paramKindCode))
                            {
                                var propDic = propParamContainer[paramKindCode];

                                var iter2 = propDic.GetEnumerator();

                                while (iter2.MoveNext())
                                {
                                    var pc = iter2.Current;

                                    ex.SetPropAsParam(pc.Key, pc.Value);
                                }
                            }

                            if (paramContainer.ContainsKey(paramKindCode))
                            {
                                v = ex.Evaluate(this, paramContainer[paramKindCode]);
                            }
                            else
                            {
                                v = ex.Evaluate(this);
                            }
                        }
                        else
                        {
                            v = ex.Evaluate(this);
                        }
                        if (propInfo.calFunc == null)
                        {

                            sum += v;
                        }
                        else
                        {

                            sum = propInfo.calFunc(sum, v);
                        }


                    }

                }
            }




            if (LayerValues.ContainsKey(pName))
            {
                var dic = LayerValues[pName];

                PropInfo propInfo = null;
                if (propInfos.ContainsKey(pName))
                {
                    propInfo = propInfos[pName];
                }
                else
                {
                    propInfo = null;
                }



                var iter = dic.GetEnumerator();
                while(iter.MoveNext())
                {
                    var d = iter.Current;
                    if (propInfo == null || propInfo.calFunc == null)
                    {

                        sum += d.Value;
                    }
                    else
                    {

                        sum = propInfo.calFunc(sum, d.Value);
                    }

                }

            }




            ChainSet(pName, sum);
        }

        private bool IsNotEnoughParam(Dictionary<int, HashSet<ExpressionWrapper>> expressions)
        {
            int i;
            var iter = expressions.GetEnumerator();

            while(iter.MoveNext())
            {

                var p = iter.Current;
                foreach (var p1 in p.Value)
                {
                    foreach (var param in p1.parameters)
                    {
                        if (Has(param) == false)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }



        public void ChainSet(string pName, double value)
        {


            Set(pName, value);

            if (propInfos.ContainsKey(pName) == false)
                return;



            var propInfo = propInfos[pName];
            foreach (var p in propInfo.chainProps)
            {
                UpdateProperty(p);
            }
        }


        public void LinkPropsAndCopyParams(PropertiesData data)
        {

            data.propInfos = propInfos;
            data.paramContainer = new Dictionary<int, Dictionary<string, double>>();
            int i;

            {
                var iter = paramContainer.GetEnumerator();

                while (iter.MoveNext())
                {
                    var pc = iter.Current;
                    data.paramContainer.Add(pc.Key, new Dictionary<string, double>(pc.Value));
                }

            }

            data.propParamContainer = new Dictionary<int, Dictionary<string, PropertiesData>>();



            {
                var iter = propParamContainer.GetEnumerator();

                while (iter.MoveNext())
                {          
                    var pc = iter.Current;
                    data.propParamContainer.Add(pc.Key, new Dictionary<string, PropertiesData>(pc.Value));
                }
            }



            data.properties = new Dictionary<string, double>(properties);



        }


        public IEnumerable<string> GetKeys()
        {
            return properties.Keys;
        }

        ~PropertiesData()
        {
            propContainer.Clear();

        }
    }

}