using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Dynamic;
using System.Threading;
using System.Reflection.Emit;

namespace FormBot.BAL
{
    public static class DBClient
    {
        /// <summary>
        /// Data the table to list.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="table">The table.</param>
        /// <returns>list</returns>
        public static List<T> DataTableToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();

                foreach (var row in table.AsEnumerable())
                {
                    T obj = new T();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                            //propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                            if (prop.PropertyType.Name != "Nullable`1")
                            {
                                propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                            }
                            else
                            {
                                propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], Nullable.GetUnderlyingType(propertyInfo.PropertyType)), null);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Adds the parameters.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="paramType">Type of the parameter.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <returns>Param</returns>
        public static SqlParameter AddParameters(string paramName, SqlDbType paramType, object paramValue)
        {
            SqlParameter param = new SqlParameter();
            param.ParameterName = "@" + paramName;
            param.SqlDbType = paramType;
            param.Value = paramValue;
            return param;
        }

        public static List<T> ToListof<T>(this DataTable dt)
        {
            const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Instance;
            var columnNames = dt.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .ToList();
            var objectProperties = typeof(T).GetProperties(FLAGS);
            var targetList = dt.AsEnumerable().Select(dataRow =>
            {
                var instanceOfT = Activator.CreateInstance<T>();

                foreach (var properties in objectProperties.Where(properties => columnNames.Contains(properties.Name) && dataRow[properties.Name] != DBNull.Value))
                {
                    properties.SetValue(instanceOfT, dataRow[properties.Name], null);
                }
                return instanceOfT;
            }).ToList();

            return targetList;
        }
    }
    
    public static class DataTableExtension
    {
        /// <summary>
        ///  Convert a database data table to a runtime dynamic definied type collection (dynamic class' name as table name).
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static List<dynamic> ToDynamicList(DataTable dt, string className)
        {
            return ToDynamicList(ToDictionary(dt), getNewObject(dt.Columns, className));
        }

        private static List<Dictionary<string, object>> ToDictionary(DataTable dt)
        {
            var columns = dt.Columns.Cast<DataColumn>();
            var Temp = dt.AsEnumerable().Select(dataRow => columns.Select(column =>
                                 new { Column = column.ColumnName, Value = dataRow[column] })
                             .ToDictionary(data => data.Column, data => data.Value)).ToList();
            return Temp.ToList();
        }

        private static List<dynamic> ToDynamicList(List<Dictionary<string, object>> list, Type TypeObj)
        {
            dynamic temp = new List<dynamic>();
            foreach (Dictionary<string, object> step in list)
            {
                object Obj = Activator.CreateInstance(TypeObj);

                PropertyInfo[] properties = Obj.GetType().GetProperties();
                Dictionary<string, object> DictList = (Dictionary<string, object>)step;

                foreach (KeyValuePair<string, object> keyValuePair in DictList)
                {
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.Name == keyValuePair.Key)
                        {
                            if (keyValuePair.Value != null && keyValuePair.Value.GetType() != typeof(System.DBNull))
                            {
                                if (keyValuePair.Value.GetType() == typeof(System.Guid))
                                {
                                    property.SetValue(Obj, keyValuePair.Value, null);
                                }
                                else
                                {
                                    property.SetValue(Obj, keyValuePair.Value, null);
                                }
                            }
                            else if(keyValuePair.Value == DBNull.Value)
                            {
                                property.SetValue(Obj, null, null);
                            }
                            break;
                        }
                    }
                }
                temp.Add(Obj);
            }
            return temp;
        }

        private static Type getNewObject(DataColumnCollection columns, string className)
        {
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "YourAssembly";
            System.Reflection.Emit.AssemblyBuilder assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder module = assemblyBuilder.DefineDynamicModule("YourDynamicModule");
            TypeBuilder typeBuilder = module.DefineType(className, TypeAttributes.Public);

            foreach (DataColumn column in columns)
            {
                string propertyName = column.ColumnName;
                FieldBuilder field = typeBuilder.DefineField(propertyName, (column.DataType != typeof(string) && column.AllowDBNull) ? typeof(Nullable<>).MakeGenericType(column.DataType):column.DataType, FieldAttributes.Public);
                PropertyBuilder property = typeBuilder.DefineProperty(propertyName, System.Reflection.PropertyAttributes.None, column.DataType, new Type[] { (column.AllowDBNull && column.DataType != typeof(string))? typeof(Nullable<>).MakeGenericType(column.DataType) : column.DataType });
                MethodAttributes GetSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig;
                MethodBuilder currGetPropMthdBldr = typeBuilder.DefineMethod("get_value", GetSetAttr,column.DataType, new Type[] { (column.AllowDBNull && column.DataType != typeof(string)) ? typeof(Nullable<>).MakeGenericType(column.DataType) : column.DataType }); // Type.EmptyTypes);
                ILGenerator currGetIL = currGetPropMthdBldr.GetILGenerator();
                currGetIL.Emit(OpCodes.Ldarg_0);
                currGetIL.Emit(OpCodes.Ldfld, field);
                currGetIL.Emit(OpCodes.Ret);
                MethodBuilder currSetPropMthdBldr = typeBuilder.DefineMethod("set_value", GetSetAttr, null, new Type[] { (column.AllowDBNull && column.DataType != typeof(string)) ? typeof(Nullable<>).MakeGenericType(column.DataType) : column.DataType });
                ILGenerator currSetIL = currSetPropMthdBldr.GetILGenerator();
                currSetIL.Emit(OpCodes.Ldarg_0);
                currSetIL.Emit(OpCodes.Ldarg_1);
                currSetIL.Emit(OpCodes.Stfld, field);
                currSetIL.Emit(OpCodes.Ret);
                property.SetGetMethod(currGetPropMthdBldr);
                property.SetSetMethod(currSetPropMthdBldr);
            }
            Type obj = typeBuilder.CreateType();
            return obj;
        }
    }
}
