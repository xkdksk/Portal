
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System.Text;

namespace UJ.Data
{

    public static class UtilExt
    {

        //https://codereview.stackexchange.com/questions/13105/integer-to-alphabet-string-a-b-z-aa-ab
        static readonly string[] Columns = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ", "BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH" };
        public static string IndexToColumn(int index)
        {
            if (index <= 0)
                throw new IndexOutOfRangeException("index must be a positive number");

            return Columns[index - 1];
        }



        public static void SetAlpha(this UnityEngine.UI.Image img, float alpha)
        {

            var c = img.color;
            c.a = alpha;
            img.color = c;
        }

        public static bool NoTouch()
        {
            return Input.GetMouseButtonDown(0) == false && Input.GetMouseButtonUp(0) == false && Input.touchCount <= 0;
        }


        public static UnityEngine.Vector3 BezierCurve(float t, UnityEngine.Vector3 p0, UnityEngine.Vector3 p1)
        {
            return ((1 - t) * p0) + ((t) * p1);
        }

        public static UnityEngine.Vector3 BezierCurve(float t, UnityEngine.Vector3 p0, UnityEngine.Vector3 p1, UnityEngine.Vector3 p2)
        {
            var pa = BezierCurve(t, p0, p1);
            var pb = BezierCurve(t, p1, p2);
            return BezierCurve(t, pa, pb);
        }
        public static UnityEngine.Vector3 BezierCurve(float t, UnityEngine.Vector3 p0, UnityEngine.Vector3 p1, UnityEngine.Vector3 p2, UnityEngine.Vector3 p3)
        {
            var pa = BezierCurve(t, p0, p1);
            var pb = BezierCurve(t, p1, p2);
            var pc = BezierCurve(t, p2, p3);
            return BezierCurve(t, pa, pb, pc);
        }




        public static string ToSimpleStr(this BigInteger i)
        {
            var l = BigInteger.Log(i, 10);
            var a = (int)(l / 3);

            if (a > 0)
            {



                return ((double)BigInteger.Divide(i, BigInteger.Pow(10, a)) / 100f) + IndexToColumn(a);
            }
            else
            {
                return i.ToString();
            }
        }


        public static string ToSimpleStr(this double i)
        {
            var l = Math.Log(i, 10);
            var a = (int)(l / 3);

            if (a > 0)
            {



                return ((int)(i / Math.Pow(10, a)) / 100f) + IndexToColumn(a);
            }
            else
            {

                return string.Format("{0:f2}", i);


            }
        }



        public static string ToSimpleStrKM(this int i)
        {
            var l = Math.Log(i, 10);

            if (l > 3)
            {
                return Mathf.FloorToInt(i * 0.001f) + "K";
            }
            else
            {
                return i.ToString();
            }
        }



        public static string ToSimpleStr(this int i)
        {
            var l = Math.Log(i, 10);
            var a = (int)(l / 3);

            if (a > 0)
            {



                return ((int)(i / Math.Pow(10, a)) / 100f) + IndexToColumn(a);
            }
            else
            {
                return ((int)i).ToString();
            }
        }


        public static T GetOrCreate<T>(this GameObject obj) where T : Behaviour
        {

            var t = obj.GetComponent<T>();
            if (t == null)
            {
                return obj.AddComponent<T>();
            }

            return t;
        }


        public static TVal GetValOrDefault<TKey, TVal>(this IDictionary<TKey, TVal> dic, TKey key)
        {
            if (!dic.ContainsKey(key))
            {
                return default(TVal);
            }
            return dic[key];
        }



        static List<RaycastResult> PointerOverUiResult = new List<RaycastResult>();
        public static bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);

            eventDataCurrentPosition.position = new UnityEngine.Vector2(Input.mousePosition.x, Input.mousePosition.y);
            PointerOverUiResult.Clear();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, PointerOverUiResult);
            return PointerOverUiResult.Count > 0;
        }

        public static bool IsAllTouchPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);

            foreach (var t in Input.touches)
            {
                eventDataCurrentPosition.position = new UnityEngine.Vector2(t.position.x, t.position.y);
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
                if (results.Count == 0)
                {
                    return false;
                }
            }
            return true;



        }

        public static int GetRandomIdx(params float[] rates)
        {

            var sum = rates.Sum();
            var rv = UnityEngine.Random.value * sum;

            int i;
            for (i = 0; i < rates.Length; i++)
            {
                rv -= rates[i];
                if (rv <= 0)
                {

                    return i;
                }
            }

            return -1;
        }



        public static int FirstDigit(this int x)
        {
            if (x == 0) return 0;
            x = Mathf.Abs(x);
            return (int)Mathf.Floor(x / Mathf.Pow(10, Mathf.Floor(Mathf.Log10(x))));
        }

        public static Touch GetTouchNotOnUI()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);

            foreach (var t in Input.touches)
            {
                eventDataCurrentPosition.position = new UnityEngine.Vector2(t.position.x, t.position.y);
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
                if (results.Count == 0)
                {
                    return t;
                }

            }
            return new Touch();



        }



        public static string FormatStr(this string str, Func<string, string> GetValFunc)
        {
            StringBuilder sb = new StringBuilder();
            int idx = 0;

            while (idx < str.Length)
            {
                var idx2 = str.IndexOf('{', idx);

                if (idx2 == -1)
                {
                    sb.Append(str.Substring(idx));


                    break;
                }

                var strPart = str.Substring(idx, idx2 - idx);
                sb.Append(strPart);

                idx = idx2 + 1;
                var symbolEndIdx = str.IndexOf('}', idx);

                if (symbolEndIdx == -1)
                {

                    sb.Append(str.Substring(idx2));

                    break;
                }


                var symbol = str.Substring(idx, symbolEndIdx - idx);


                string symbolVal = GetValFunc(symbol);
                sb.Append(symbolVal);
                idx = symbolEndIdx + 1;

            }

            return sb.ToString();
        }

    }


}