
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UJ.Data
{

    [Serializable]
    public class EtcStr
    {
        public string code;
        public string kor, eng, jp, ch, sp;
        public override string ToString()
        {
            switch (Str.CurrentLanguage)
            {
                case Str.Language.Kor:
                    return kor;
                case Str.Language.Eng:
                    return eng;
            }

            return kor;
        }

        static List<EtcStr> _Strs;

        public static void SetEtcStrs(List<EtcStr> strs)
        {
            _Strs = strs;
        }

        public static List<EtcStr> Strs => _Strs;

        public static string FindStr(string code, params object[] parameters )
        {
            var str= _Strs.Find(l => l.code == code);

            if (str != null)
            {
                return string.Format(str.ToString(), parameters);
            }

            return string.Format("[{0}]", code);
        }
 
    }



    [Serializable]
    public class ExtStr :Str
    {
        public string info;
    }


    [Serializable]
    public class Str
    {
        public int code;
    
        public enum Language
        {
            Eng,Kor
        }
        public static Language CurrentLanguage;
        public int kindCode;

        public string kor,eng;


        static List<Str> _Strs;

        public static List<Str> Strs => _Strs;

        public static Str FindStr(int code, int kind)
        {
            return _Strs.Find(l => l.code == code && l.kindCode == kind);
        }


        public static void SetStrs(List<Str> strs)
        {
            _Strs = strs;

        }

        public override string ToString()
        {

            switch (CurrentLanguage)
            {
                case Language.Eng:
                    return eng;
                case Language.Kor:
                    return kor;
            }

            return "";

        }

    }


    [System.AttributeUsage(System.AttributeTargets.Field |
           System.AttributeTargets.Property)]
    public class IgnoreFromXlsx : System.Attribute
    {

    }



    [System.AttributeUsage(System.AttributeTargets.Field |
               System.AttributeTargets.Property)]

    public class MultiLanguage : System.Attribute
    {
        public class UsingFields
        {
            public Type hostType;
            public List<string> fieldNames= new List<string>();

        }

        static Dictionary<string, int> MultiLangDic = new Dictionary<string, int>();
        static List<UsingFields> usedTypes = new List<UsingFields>();
       

        public static string GetTypeName(Type parentType, FieldInfo f)
        {
            return parentType.Name + "_" + f.Name;
        }

        public static string GetTypeName(Type parentType, string fName)
        {
            return parentType.Name + "_" + fName;
        }
        public static int GetCode(Type parentType, FieldInfo f)
        {
            return MultiLangDic[GetTypeName(parentType, f)];
        }

        public static string GetTableAndField(int code)
        {
            return MultiLangDic.First(l => l.Value == code).Key;

        }

        /*
        public static string GenerateStrKindCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("public enum StrKind{\n");
            var last = MultiLangDic.Last();
            foreach (var kp in MultiLangDic)
            {
                sb.Append("\t");
                sb.Append(kp.Key);
                sb.Append("=");
                sb.Append(kp.Value);
                if (last.Value != kp.Value)
                {
                    sb.Append(",");
                }
                sb.Append("\n");
            }

            sb.Append("}\n");

            return sb.ToString();

        }
        */

        public static string GenerateParticalClassCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("using UJ.Data;\n");
            bool isInNamespace = false;
            var nameSpaces = usedTypes.Select(l => l.hostType.Namespace).Distinct();
            if (nameSpaces.Count() == 1)
            {
                var ns = nameSpaces.First();
                if (ns != null) {
                    isInNamespace = true;
                    sb.AppendLine("namespace "+ns+"{");
                }
            }
            else
            {
                foreach (var n in nameSpaces)
                {
                    sb.Append("using ");
                    sb.Append(n);
                    sb.AppendLine(";");
                }
            }



            foreach (var ut in usedTypes)
            {
          

                sb.Append("public partial class ");
                sb.Append(ut.hostType.Name);
                sb.Append("{\n");

                foreach (var f in ut.fieldNames) {

                    var farr=f.ToCharArray();
                    farr[0] = Char.ToUpper(farr[0]);
                    string propName = new String(farr);

                    string kindName = ut.hostType.Name + "_" + f;

                    var code = GetCode(ut.hostType, ut.hostType.GetField(f));

                    sb.AppendLine("\n\t[System.NonSerialized]");
                    sb.AppendFormat("\n\tStr _{0};\n", propName);
                    sb.Append      ("\t public Str "+propName+"  {\n");
                    sb.Append      ("\t\tget{\n");
                    sb.Append      ("\t\t\tif(_"+propName+"==null){\n");
                    sb.Append("\t\t\t\t_" + propName + "= Str.FindStr(code ," + code+" );\n") ;
                    sb.Append      ("\t\t\t}\n");
                    sb.Append      ("\t\t\t return _" + propName + ";\n");
                    sb.Append      ("\t\t}\n");
                    sb.Append      ("\t}\n\n");
                        
                }



                sb.AppendLine("}");

            }

            if (isInNamespace)
            {
                sb.AppendLine("}");
            }

            return sb.ToString();

        }



        public static List<Str> ExtractStrs<T>( T t)
        {
            List<Str> result = new List<Str>();
            var type = typeof(T);
            var ut=usedTypes.Find(l => l.hostType == type);

            if (ut != null)
            {
                ut.fieldNames.Clear();
            }
            else
            {
                ut = new UsingFields()
                {
                    hostType = type
                };
                usedTypes.Add(ut);

            }



            foreach (var f in typeof(T).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                if (f.GetCustomAttributes(false).Any(l => l is MultiLanguage))
                {

                    var eng = (string)f.GetValue(t);

                    var code =(int)t.GetType().GetField("code").GetValue(t);

                    ut.fieldNames.Add(f.Name);

                    result.Add(new Str() {
                        code= code,
               //         kind=  GetTypeName(typeof(T),f) ,
                        kindCode = GetCode(typeof(T), f),
                        eng =eng
                    });
                  
                }
            }

            return result;
        }



        public class TableField
        {
            public string tableName, fieldName;
        }

        public static void FillMultiLangDic(IEnumerable<Type> types)
        {

            MultiLangDic.Clear();

            foreach (var type in types)
            {

                foreach (var f in type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
                {
                    if (f.GetCustomAttributes(false).Any(l => l is MultiLanguage))
                    {
                        Console.WriteLine("Type " + type.Name + " " + f.Name);
                        
                        MultiLangDic.Add(GetTypeName(type,f), MultiLangDic.Count+1);

                    }
                }
            }

        }

    }
}
