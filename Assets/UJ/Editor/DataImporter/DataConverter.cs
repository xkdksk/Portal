using ClosedXML.Excel;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UJ.Data;

namespace UJ.Data.Editor
{
 

    public class readInfo
    {
        Dictionary<string, Dictionary<int, object>> objs = new Dictionary<string, Dictionary<int, object>>();


        public static int lastWorkRow;
        public static string lastWorkSheet;
        public static int lastWorkColumn;

        public void ObjectFromExcel<T>(XLWorkbook workbook,string sheetName, out List<T> obj)
        {
            IXLWorksheet worksheet;
            if (workbook.TryGetWorksheet(sheetName, out worksheet) == false)
            {
                Debug.LogError(sheetName + " 테이블이 엑셀에 없음  없음!");
                throw new Exception("테이블 없음");
            }

            var type = typeof(T);

            Debug.Log("ObjectFromExcel " + sheetName + " " + type.Name);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            int i;
            int row = 2;

            List<T> result = new List<T>();
            obj = result;

            int emptyLine = 0;
            while (true)
            {
                var cell = worksheet.Cell(row, 1);
                var v = cell.GetValue<string>();

                if (string.IsNullOrEmpty(v))
                {
                    row++;
                    emptyLine++;
                    if (emptyLine >= 2)
                    {
                        break;
                    }
                    continue;
                }

                emptyLine = 0;
                result.Add((T)readLine(typeof(T), row, workbook, worksheet));
                row++;
            }
        }



        public void ObjectFromExcel<T>(string path,string sheetName, out List<T> obj)
        {
            XLWorkbook workbook = GetWorkbookFromPath(path);
            ObjectFromExcel<T>(workbook, sheetName, out obj);
        }

        public static XLWorkbook GetWorkbookFromPath(string path)
        {
            Debug.Log("path " + path);
            XLWorkbook workbook=null;
            if( File.Exists(path)==false)
            {
                return null;
            }
    

                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    workbook = new XLWorkbook(stream);
                    stream.Close();
                }
          






            return workbook;
        }



        private object readLine(Type type, int row, XLWorkbook workbook, IXLWorksheet worksheet)
        {
            object rowObj = Activator.CreateInstance(type);
            lastWorkSheet = worksheet.Name;
            lastWorkRow = row;


      

            int column = 1;
            while (true)
            {
                string columnName = worksheet.Cell(1, column).Value as String;

       


                columnName = columnName.Trim();

                if (string.IsNullOrEmpty(columnName))
                    break;
                lastWorkColumn = column;
                var value = worksheet.Cell(row, column).Value;

                bool isListColumn= columnName.EndsWith("[]");
                if (isListColumn)
                {
                    columnName=columnName.Substring(0, columnName.Length - 2);
                    
                }


                column++;
                var f = type.GetField(columnName);
                if (f == null)
                    continue;

                if (f.FieldType.IsClass)
                {
                    if (f.FieldType.Name != "String")
                    {

                        if (value is string && string.IsNullOrEmpty(value as string))
                            continue;
                        object o;

                        if (isListColumn)
                        {
                            var list = (IList)f.GetValue(rowObj);

                            if(list== null)
                            {
                                try
                                {
                                    list = (IList)Activator.CreateInstance(f.FieldType);
                                }
                                catch(Exception e)
                                {
                                    Debug.LogError("e "  + type + " " + e);
                                }
                            }
                            object v;
                            if (f.FieldType.IsEnum && value is string)
                            {
                                v = Enum.Parse(f.FieldType, (string)value);
                            }
                            else
                            {
                                //v = o = readObject(f.FieldType, f.FieldType.Name, id, workbook); ;
                                var ty = f.FieldType.GetGenericArguments()[0];
                              
                                if (ty.IsClass && ( ty.Namespace==null || ty.Namespace.StartsWith("System")==false))
                                {
                                    v = readObject(ty, ty.Name, Convert.ToInt32(value), workbook);
                                }                            
                                else
                                {
                                    v = Convert.ChangeType(value, ty);
                                }
                           
                            }

                            list.Add(v);

                            f.SetValue(rowObj, list);


                            continue;
                        }


                        int id = Convert.ToInt32(value);
                        if (f.FieldType.GetInterface("IList") == null)
                        {
                            o = readObject(f.FieldType, f.FieldType.Name, id, workbook);
                        }
                        else
                        {
                            var ty = f.FieldType.GetGenericArguments()[0];

                            o = readList(f.FieldType, ty.Name, id, workbook);
                        }


                        f.SetValue(rowObj, o);
                        continue;
                    }
                }

                if (f.FieldType.IsEnum && value is string)
                {
                    try
                    {
                        f.SetValue(rowObj, Enum.Parse(f.FieldType, (string)value));
                    }catch(Exception e)
                    {
                        Debug.LogError("f " + f.Name + " "+value);
                        Debug.LogError("Type " + type + " row" + row); 
                        Debug.LogError("e " + e);
                    }
                }
                else
                {
                    try
                    {
                        f.SetValue(rowObj, Convert.ChangeType(value, f.FieldType));
                    }
                    catch (Exception)
                    {
                        Debug.LogError("마지막시트  " + lastWorkSheet+ "  " + lastWorkRow + "행 변환에러");
                    }
                }


            }


            return rowObj;
        }

        private object readList(Type listType, string name, int id, XLWorkbook workbook)
        {
            Type type;
            TypeHelper.TryListOfWhat(listType, out type);



            Dictionary<int, object> dic;
            if (!objs.ContainsKey(name))
            {
                IXLWorksheet sheet;
                lastWorkSheet = name;
                workbook.TryGetWorksheet(name, out sheet);

                objs.Add(name, new Dictionary<int, object>());
                dic = objs[name];

                int row = 2;
                bool oneLineSkipped = false;
                while (true)
                {
                    var cell = sheet.Cell(row, 1);
                    string idStr = cell.GetValue<string>();

                    if (string.IsNullOrEmpty(idStr))
                    {
                        if (oneLineSkipped)
                        {
                            break;
                        }
                        oneLineSkipped = true;
                        row++;
                        continue;
                    }
                    oneLineSkipped = false;
                    int rowId = cell.GetValue<int>();

                    if (cell.ValueCached != null && cell.ValueCached != cell.Value)
                    {
                        rowId = int.Parse(cell.ValueCached);
                    }



                    if (dic.ContainsKey(rowId) == false)
                    {
                        var t_listType = typeof(List<>);
                        var constructedListType = t_listType.MakeGenericType(type);

                        IList newList = (IList)Activator.CreateInstance(constructedListType);
                        dic.Add(rowId, newList);
                    }

                    var list = (IList)dic[rowId];

                    if (type.IsPrimitive)
                    {
                        list.Add(Convert.ChangeType(sheet.Cell(row, 2).Value, type));
                    }
                    else
                    {
                        list.Add(readLine(type, row, workbook, sheet));
                    }
                    row++;
                }


            }
            dic = objs[name];

            if (listType.IsArray)
            {
                var resultList = dic[id] as IList;

                var array = Array.CreateInstance(type, resultList.Count);

                int i = 0;
                foreach (var r in resultList)
                {
                    array.SetValue(r, i);
                    i++;

                }

                return array;

            }

            if(dic.ContainsKey(id))
                return dic[id];

            return null;


        }

        private object readObject(Type type, string name, int id, XLWorkbook workbook)
        {
            Dictionary<int, object> dic;
            if (!objs.ContainsKey(name))
            {
                IXLWorksheet sheet;
                workbook.TryGetWorksheet(name, out sheet);
                objs.Add(name, new Dictionary<int, object>());
                dic = objs[name];

                int row = 2;
                bool oneLineSkipped=false;
                while (true)
                {
                    string idStr = sheet.Cell(row, 1).GetValue<string>();

                    if (string.IsNullOrEmpty(idStr))
                    {
                        if (oneLineSkipped)
                        {
                            break;
                        }
                        oneLineSkipped = true;
                        row++;
                        continue;
                    }
                    oneLineSkipped = false;
                    int rowId = sheet.Cell(row, 1).GetValue<int>();

                    if (dic.ContainsKey(rowId)==false)
                    {
                        dic.Add(rowId, readLine(type, row, workbook, sheet));
                    }
                    row++;
                }
            }
            dic = objs[name];

            return dic[id];

        }
    }

    public class writeInfo
    {
        Dictionary<string, int> objId = new Dictionary<string, int>();



        public void ObjectToExcel<T>(XLWorkbook workbook, List<T> obj)
        {

            var sheetName = typeof(T).Name;
            if (workbook.Worksheets.Any(l => l.Name == sheetName))
            {
                workbook.Worksheets.Delete(sheetName);
            }
            var worksheet = workbook.Worksheets.Add(sheetName);

            var type = typeof(T);

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            fields = fields.OrderBy(l =>
            {
                if (l.Name == "code")
                {
                    return "";
                }


                return l.Name;
            }).ToArray();

            int i;
            for (i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                worksheet.Cell(1, i + 1).Value = f.Name;

            }

            for (i = 0; i < obj.Count; i++)
            {

                writeLine(0, i + 2, obj[i], fields, workbook, worksheet);
            }


        }


        public void ObjectToExcel<T>(string fileName, List<T> obj)
        {
            var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add(typeof(T).Name);

            var type = typeof(T);

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);


            int i;
            for (i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                worksheet.Cell(1, i + 1).Value = f.Name;

            }

            for (i = 0; i < obj.Count; i++)
            {

                writeLine(0, i + 2, obj[i], fields, workbook, worksheet);
            }



            workbook.SaveAs(fileName);
        }

        private void writeLine(int columOffset, int row, object t, FieldInfo[] fields, XLWorkbook workbook, IXLWorksheet worksheet)
        {
            int i;
            for (i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                var currentCell = worksheet.Cell(row, columOffset + i + 1);

                if (f.FieldType.IsClass)
                {
                    if (f.FieldType.Name != "String")
                    {
                        var obj = f.GetValue(t);
                        if (obj == null)
                            continue;
                        if (f.FieldType.GetInterface("IList") == null)
                        {
                            writeObject(obj, workbook, currentCell);
                        }
                        else
                        {
                            writeList(f.Name, obj as IList, workbook, currentCell);
                        }
                        continue;
                    }
                }
                currentCell.Value = f.GetValue(t);
            }

        }



        private void writeList(string fieldName, IList list, XLWorkbook workbook, IXLCell currentCell)
        {
            IXLWorksheet sheet;

            var tt = list.GetType();
            Type type;
            TypeHelper.TryListOfWhat(list.GetType(), out type);


            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            if (workbook.TryGetWorksheet(fieldName, out sheet) == false)
            {
                sheet = workbook.Worksheets.Add(fieldName);
                objId.Add(fieldName, 0);

                int i;
                sheet.Cell(1, 1).Value = "_id";
                for (i = 0; i < fields.Length; i++)
                {
                    var f = fields[i];
                    sheet.Cell(1, i + 2).Value = f.Name;
                }
            }

            int cnt = objId[fieldName];
            objId[fieldName]++;
            int id = cnt + 1;
            int row = sheet.LastRowUsed().RowNumber() + 1;



            foreach (var obj in list)
            {
                currentCell.Value = id;
                sheet.Cell(row, 1).Value = id;

                if (fields.Length == 0)
                {
                    sheet.Cell(row, 2).Value = obj;
                }
                else
                {
                    writeLine(1, row, obj, fields, workbook, sheet);
                }


                row++;
            }
        }

        private void writeObject(object obj, XLWorkbook workbook, IXLCell currentCell)
        {
            IXLWorksheet sheet;

            var type = obj.GetType();
            var typeName = type.Name;
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            if (workbook.TryGetWorksheet(typeName, out sheet) == false)
            {
                sheet = workbook.Worksheets.Add(typeName);
                objId.Add(typeName, 0);

                int i;
                sheet.Cell(1, 1).Value = "_id";
                for (i = 0; i < fields.Length; i++)
                {
                    var f = fields[i];
                    sheet.Cell(1, i + 2).Value = f.Name;
                }
            }

            int cnt = objId[typeName];
            int id = cnt + 1;
            int row = sheet.LastRowUsed().RowNumber() + 1;
            objId[typeName]++;

            currentCell.Value = id;
            sheet.Cell(row, 1).Value = id;
            writeLine(1, row, obj, fields, workbook, sheet);

        }
    }




    public static class TypeHelper
    {
        public static bool TryListOfWhat(Type type, out Type innerType)
        {
            //Contract.Requires(type != null);

            var interfaceTest = new Func<Type, Type>(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>) ? i.GetGenericArguments().Single() : null);

            innerType = interfaceTest(type);
            if (innerType != null)
            {
                return true;
            }

            foreach (var i in type.GetInterfaces())
            {
                innerType = interfaceTest(i);
                if (innerType != null)
                {
                    return true;
                }
            }

            return false;
        }
    }



    public class Reader
    {


        public static List<T> ReadFromExcel<T>(XLWorkbook workBook,string sheetName)
        {
            readInfo ri = new readInfo();
            List<T> l;
            ri.ObjectFromExcel(workBook, sheetName, out l);
            return l;
        }



        public static void LoadFromFile<T>(T gd, string fromPath) where T : ScriptableObject
        {
            var workbook = readInfo.GetWorkbookFromPath(fromPath);
            var readerType= typeof(Reader);

            var gdType = typeof(T);
            var fields = gdType.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            foreach (var f in fields)
            {
                var fieldType = f.FieldType;

                if (fieldType.GenericTypeArguments.Length == 0)
                    continue;
                if (f.Name == "Strs" || f.Name == "EtcStrs" || f.Name=="_Strs")
                    continue;



                var argType = fieldType.GenericTypeArguments[0];



                if (f.CustomAttributes.Any(a => a.AttributeType.Name == "IgnoreFromXlsx" || a.AttributeType.Name== "NonSerialized") ||
                    f.IsNotSerialized)
                {
            ///        var method = this.GetType().GetMethod("JsonToObj", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

               ///     method = method.MakeGenericMethod(argType);

                  ///  f.SetValue(gd, method.Invoke(gd, new object[] { toPath }));
                    continue;
                }

                {


                    var method = readerType.GetMethod("ReadFromExcel", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

                    method = method.MakeGenericMethod(argType);

                    f.SetValue(gd, method.Invoke(gd, new object[] { workbook , f.Name}));
                }
            }
        }



        public static void FillStrs<T>(T gd,List<Str> strs) where T:ScriptableObject
        {



            var gdType = typeof(T);
            var fields = gdType.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            List<Str> result = new List<Str>();

            foreach (var f in fields)
            {
                var fieldType = f.FieldType;

                if (fieldType.GenericTypeArguments.Length == 0)
                    continue;
                var argType = fieldType.GenericTypeArguments[0];
                if (argType.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                    .Any(l => l.CustomAttributes.Any(a => a.AttributeType.Name == "MultiLanguage")) == false)
                {
                    continue;
                }


                var method = typeof(MultiLanguage).GetMethod("ExtractStrs", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                method = method.MakeGenericMethod(argType);

                var param = (IList)f.GetValue(gd);
                foreach (var p in param)
                {
                    var list = (List<Str>)method.Invoke(gd, new object[] { p });
                    result.AddRange(list);
                }

            }
            var newResult = result;
         
            foreach (var row in newResult)
            {
                var r = strs.FirstOrDefault(l => l.code == row.code && l.kindCode == row.kindCode);

                if (r == null)
                {
                    strs.Add(row);
                }
                else
                {
                    r.kindCode = row.kindCode;
                    r.kor = row.kor;
                }


            }

            strs.RemoveAll(l => result.Any(r => r.code == l.code && r.kindCode == l.kindCode) == false);


        }


    }



}
