using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CompanyDal
{
    public static class CommonDal
    {
        public static string DefaultConnectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["Dbconnection"].ConnectionString);
        private static string SessionConnectionString = "";
        /// <summary>
        /// Executes the data set main.
        /// </summary>
        /// <param name="spName">Name of the sp.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <returns>Data Set</returns>
        public static DataSet ExecuteDataSet(string spName, params SqlParameter[] commandParameters)
        {
            return ExecuteDataSetMain(spName, DefaultConnectionString, commandParameters);
        }

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="spName">Name of the sp.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <returns>object</returns>
        public static object ExecuteScalar(string spName, params SqlParameter[] commandParameters)
        {
            return ExecuteScalarMain(spName, DefaultConnectionString, commandParameters);
        }

        /// <summary>
        /// You can Directly Cast the model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <returns>IList<T></returns>
        public static IList<T> ExecuteProcedure<T>(string procedureName, params SqlParameter[] commandParameters)
        {
            return ExecuteProcedureMain<T>(procedureName, DefaultConnectionString, commandParameters);
        }

        /// <summary>
        /// Return the Object of Model with data for given primary key value
        /// </summary>
        /// <typeparam name="TEntity">T Entity</typeparam>
        /// <param name="procedureName">procedure Name</param>
        /// <param name="commandParameters">command Parameters</param>
        /// <returns>Select Object</returns>
        public static TEntity SelectObject<TEntity>(string procedureName, params SqlParameter[] commandParameters)
        {
            TEntity entityObject = default(TEntity);
            entityObject = Activator.CreateInstance<TEntity>();
            IList<TEntity> result = ExecuteProcedure<TEntity>(procedureName, commandParameters);
            if (result.Count() > 0)
            {
                return result.FirstOrDefault();
            }

            return entityObject;
        }

        /// <summary>
        /// Cruds the specified sp name.
        /// </summary>
        /// <param name="spName">Name of the sp.</param>
        /// <param name="commandParameters">The command parameters.</param>
        public static void Crud(string spName, params SqlParameter[] commandParameters)
        {
            CrudMain(spName, DefaultConnectionString, commandParameters);
        }

        /// <summary>
        /// Perform the Insert Update Delete.
        /// </summary>
        /// <param name="spName">sp Name</param>
        /// <param name="connectionString">connection String</param>
        /// <param name="commandParameters">command Parameters</param>
        public static void CrudMain(string spName, string connectionString, params SqlParameter[] commandParameters)
        {
            SqlCommand objCmd = null;
            SqlConnection connection = null;
            try
            {
                if (!string.IsNullOrEmpty(spName))
                {
                    using (connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        objCmd = new SqlCommand(spName, connection);
                        objCmd.CommandType = CommandType.StoredProcedure;
                        objCmd.CommandTimeout = 300;

                        if (commandParameters.Length > 0)
                        {
                            objCmd.Parameters.AddRange(commandParameters);
                        }

                        objCmd.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                DisposeOf(objCmd);
                connection.Close();
            }
        }

        /// <summary>
        /// Disposes the of.
        /// </summary>
        /// <param name="object">The object.</param>
        public static void DisposeOf(object @object)
        {
            if (@object == null)
            {
                return;
            }

            var o = @object as IDisposable;
            if (o != null)
            {
                o.Dispose();
            }
        }

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns>primary Key Value</returns>
        public static int Save<TEntity>(TEntity entity, string procedureName)
        {
            System.Collections.ObjectModel.Collection<SqlParameter> parameters = AddParameters(entity);

            /*Execute Stored Procedure*/
            object primaryKeyValue = ExecuteScalar(procedureName, parameters.ToArray());

            return Convert.ToInt32(primaryKeyValue);
        }

        /// <summary>
        /// Add Parameter 
        /// </summary>
        /// <typeparam name="TEntity">Model Type</typeparam>
        /// <param name="entity">entity of object</param>
        /// <param name="isForSearch">require for search</param>
        /// <returns>returns list of parameters</returns>
        public static System.Collections.ObjectModel.Collection<SqlParameter> AddParameters<TEntity>(TEntity entity, bool isForSearch = false)
        {
            System.Collections.ObjectModel.Collection<SqlParameter> parameters = new System.Collections.ObjectModel.Collection<SqlParameter>();
            PropertyInfo[] infos = entity.GetType().GetProperties();
            foreach (PropertyInfo info in infos)
            {
                var value = info.GetValue(entity, null);

                // Verify Property Validation and than add as paramter
                if (ParameterValidation(info, value))
                {
                    if (info.PropertyType == typeof(string))
                    {
                        /* Added by Darshit Babariya 02 September 2013
                         To add trim functionality in All string parameters                        */
                        value = value == null ? null : value.ToString().Trim();
                    }

                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = info.Name,
                        Value = value,
                        DbType = GetPropertyType(info.PropertyType)
                    });
                }
            }

            return parameters;
        }

        public static object MergeDataTable(DataTable dataTable, string procedureName, string version, int fileType, int createdBy, bool isExcel = false, int subType = 0)
        {
            return MergeDataTableMain(dataTable, DefaultConnectionString, procedureName, version, fileType, createdBy, isExcel, subType);
        }

        public static void SaveBulk(DataTable dataTable, string tableName, string[] sourceColumn, string[] destinationColumn)
        {
            SqlConnection connection = null;
            try
            {
                using (connection = new SqlConnection(DefaultConnectionString))
                {
                    connection.Open();
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.BulkCopyTimeout = 7200;
                        for (int counter = 0; counter < sourceColumn.Length; counter++)
                        {
                            bulkCopy.ColumnMappings.Add(sourceColumn[counter], destinationColumn[counter]);
                        }

                        bulkCopy.DestinationTableName = tableName;
                        bulkCopy.WriteToServer(dataTable);
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }

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
                            propertyInfo.SetValue(obj, ChangeType(row[prop.Name], propertyInfo.PropertyType));
                            ////GetValue( Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null
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

        public static object ChangeType(object value, Type conversion)
        {
            var t = conversion;

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return Convert.ChangeType(value, t);
        }

        /// <summary>
        /// Executes the data set.
        /// </summary>
        /// <param name="spName">Name of the sp.</param>
        /// <param name="useSession">if set to <c>true</c> [use session].</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <returns>Data Set</returns>
        private static DataSet ExecuteDataSet(string spName, bool useSession, params SqlParameter[] commandParameters)
        {
            SetConnectionStringFromSession();
            if (!string.IsNullOrEmpty(SessionConnectionString))
            {
                return ExecuteDataSetMain(spName, SessionConnectionString, commandParameters);
            }

            return new DataSet();
        }

        /// <summary>
        /// Executes the data set.
        /// </summary>
        /// <param name="spName">Name of the sp.</param>
        ///   /// <param name="connectionString">The connection String.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <returns>data set</returns>
        private static DataSet ExecuteDataSetMain(string spName, string connectionString, params SqlParameter[] commandParameters)
        {
            SqlConnection connection = null;
            SqlCommand objCmd = null;
            SqlDataAdapter objDa = null;
            DataSet objDs = null;
            try
            {
                if (!string.IsNullOrEmpty(spName))
                {
                    using (connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        objDs = new DataSet();
                        objCmd = new SqlCommand(spName, connection);
                        objCmd.CommandType = CommandType.StoredProcedure;
                        objCmd.CommandTimeout = 3600;
                        if (commandParameters != null && commandParameters.Length > 0)
                        {
                            objCmd.Parameters.AddRange(commandParameters);
                        }

                        objDa = new SqlDataAdapter(objCmd);
                        objDa.Fill(objDs);
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                DisposeOf(objCmd);
                DisposeOf(objDa);
                DisposeOf(objDs);
                connection.Close();
            }

            return objDs;
        }

        /// <summary>
        /// This will be used when the dynamic db login required.
        /// </summary>
        /// <param name="spName">Name of the sp.</param>
        /// <param name="useSession">if set to <c>true</c> [use session].</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <returns>Execute Scalar</returns>
        private static object ExecuteScalar(string spName, bool useSession, params SqlParameter[] commandParameters)
        {
            SetConnectionStringFromSession();
            if (!string.IsNullOrEmpty(SessionConnectionString))
            {
                return ExecuteScalarMain(spName, SessionConnectionString, commandParameters);
            }

            return null;
        }

        /// <summary>
        /// Executes the scalar main.
        /// </summary>
        /// <param name="spName">Name of the sp.</param>
        /// <param name="connectionString">The string.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <returns> obj ect </returns>
        private static object ExecuteScalarMain(string spName, string connectionString, params SqlParameter[] commandParameters)
        {
            SqlCommand objCmd = null;
            SqlConnection connection = null;
            try
            {
                if (!string.IsNullOrEmpty(spName))
                {
                    using (connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        objCmd = new SqlCommand(spName, connection);
                        objCmd.CommandType = CommandType.StoredProcedure;
                        objCmd.CommandTimeout = 300;

                        if (commandParameters != null && commandParameters.Length > 0)
                        {
                            objCmd.Parameters.AddRange(commandParameters);
                        }

                        var count = (object)objCmd.ExecuteScalar();
                        return count;
                    }
                }
            }
            finally
            {
                DisposeOf(objCmd);
                connection.Close();
            }

            return null;
        }

        /// <summary>
        /// Executes the procedure.
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="useSession">if set to <c> true </c> [use session].</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <returns>Execute Procedure</returns>
        private static IList<T> ExecuteProcedure<T>(string procedureName, bool useSession, params SqlParameter[] commandParameters)
        {
            SetConnectionStringFromSession();
            if (!string.IsNullOrEmpty(SessionConnectionString))
            {
                return ExecuteProcedureMain<T>(procedureName, SessionConnectionString, commandParameters);
            }

            return null;
        }

        /// <summary>
        /// You can Directly Cast the model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <returns>IList<T></returns>
        private static IList<T> ExecuteProcedureMain<T>(string procedureName, string connectionString, params SqlParameter[] commandParameters)
        {
            /* Create Database object
            Database db = DatabaseFactory.CreateDatabase();*/
            List<T> returnValue = new List<T>();

            // Create a suitable command type and add the required parameter
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlCommand = new SqlCommand(procedureName, connection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandTimeout = 0;
                /*Add different Parameter from Model object Property*/
                if (commandParameters != null && commandParameters.Length > 0)
                {
                    sqlCommand.Parameters.AddRange(commandParameters);
                }

                /*Execute Procedure from supplied Execution type*/
                returnValue = DataReaderToList<T>(sqlCommand.ExecuteReader());
                connection.Close();
            }

            return returnValue;
        }

        /// <summary>
        /// Cruds the specified sp name.
        /// </summary>
        /// <param name="spName">Name of the sp.</param>
        /// <param name="useSession">if set to <c>true</c> [use session].</param>
        /// <param name="commandParameters">The command parameters.</param>
        private static void Crud(string spName, bool useSession, params SqlParameter[] commandParameters)
        {
            SetConnectionStringFromSession();
            if (!string.IsNullOrEmpty(SessionConnectionString))
            {
                CrudMain(spName, SessionConnectionString, commandParameters);
            }
        }
        /// <summary>
        /// Merge Data Table
        /// </summary>
        /// <param name="dataTable">data Table</param>
        /// <param name="useSession">use Session</param>
        /// <param name="procedureName">procedure Name</param>
        /// <param name="version">The Version</param>
        /// <param name="fileType">file Type</param>
        /// <param name="createdBy">created By</param>
        /// <param name="isExcel">is Excel</param>
        /// <param name="subType">sub Type</param>
        /// <returns>Merge Data Table</returns>
        private static object MergeDataTable(DataTable dataTable, bool useSession, string procedureName, string version, int fileType, int createdBy, bool isExcel = false, int subType = 0)
        {
            return MergeDataTableMain(dataTable, DefaultConnectionString, procedureName, version, fileType, createdBy, isExcel, subType);
        }
        /// <summary>
        /// Merges the data table.
        /// </summary>
        /// <param name="dataTable">data Table</param>
        /// <param name="connectionString">connection String</param>
        /// <param name="procedureName">procedure Name</param>
        /// <param name="version">version</param>
        /// <param name="fileType">file Type</param>
        /// <param name="createdBy">created By</param>
        /// <param name="isExcel">is Excel</param>
        /// <param name="subType">sub type</param>
        /// <returns>Merge Data Table Main</returns>
        private static object MergeDataTableMain(DataTable dataTable, string connectionString, string procedureName, string version, int fileType, int createdBy, bool isExcel = false, int subType = 0)
        {
            SqlCommand command = null;
            SqlConnection connection = null;
            try
            {
                using (connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    command = new SqlCommand(procedureName, connection);
                    command.CommandTimeout = 320;
                    command.CommandType = CommandType.StoredProcedure;
                    SqlParameter param = command.Parameters.AddWithValue("@DataTable", dataTable);
                    param.SqlDbType = SqlDbType.Structured;
                    SqlParameter paramVersion = command.Parameters.AddWithValue("version", version);
                    paramVersion.SqlDbType = SqlDbType.NVarChar;
                    SqlParameter paramType = command.Parameters.AddWithValue("CERType", fileType);
                    paramType.SqlDbType = SqlDbType.TinyInt;
                    SqlParameter paramCreated = command.Parameters.AddWithValue("CreatedBy", createdBy);
                    paramCreated.SqlDbType = SqlDbType.Int;
                    if (fileType == 0)
                    {
                        SqlParameter paramIsExcel = command.Parameters.AddWithValue("IsExcel", isExcel);
                        paramIsExcel.SqlDbType = SqlDbType.Bit;
                    }

                    if (fileType == 3)
                    {
                        SqlParameter paramIsExcel = command.Parameters.AddWithValue("SubType", subType);
                        paramIsExcel.SqlDbType = SqlDbType.TinyInt;
                    }

                    SqlParameter paramCreatedDate = command.Parameters.AddWithValue("CreatedDate", DateTime.Now);
                    paramCreatedDate.SqlDbType = SqlDbType.DateTime;

                    var result = (object)command.ExecuteNonQuery();
                    connection.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                DisposeOf(command);
                connection.Close();
            }
        }

        /// <summary>
        /// Sets the connection string from session.
        /// </summary>
        private static void SetConnectionStringFromSession()
        {
            SessionConnectionString = Convert.ToString(HttpContext.Current.Session["ConnectionString"]);
            
        }

        /// <summary>
        /// Datas the reader to list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr">The dr.</param>
        /// <returns>List <T></returns>
        private static List<T> DataReaderToList<T>(IDataReader dr)
        {
            List<T> list = new List<T>();

            T obj = default(T);

            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();

                for (int i = 0; i < dr.FieldCount; i++)
                {
                    PropertyInfo info = obj.GetType().GetProperties().FirstOrDefault(o => o.Name.ToLower() == dr.GetName(i).ToLower());
                    if (info != null)
                    {
                        /*Set the Value to Model*/
                        info.SetValue(obj, dr.GetValue(i) != System.DBNull.Value ? dr.GetValue(i) : null, null);
                    }
                }

                list.Add(obj);
            }

            return list;
        }

        /// <summary>
        /// Check the Property Information and verify for adding in Parameter
        /// </summary>
        /// <param name="info">Property Info</param>
        /// <param name="value">Property Value</param>
        /// <returns>true for Add as Parameter or false</returns>
        private static bool ParameterValidation(PropertyInfo info, object value)
        {
            var notMapped = info.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute), true);

            /* Check Property is primariy key with value or not primary but has value and either Table column or complex type and with search truue */
            if (notMapped.Count() == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the Command DBType from Property
        /// </summary>
        /// <param name="type">Property Type</param>
        /// <returns>appropriate DBType</returns>
        private static DbType GetPropertyType(Type type)
        {
            // Match type Name and return DB Type
            switch (type.Name.ToLower(System.Globalization.CultureInfo.CurrentCulture))
            {
                case "byte[]":
                    return DbType.Binary;
                case "int64":
                case "int64?":
                case "long":
                case "long?":
                    return DbType.Int64;
                case "boolean":
                case "bool":
                case "bool?":
                    return DbType.Boolean;
                case "char":
                case "string":
                    return DbType.String;
                case "datetime":
                case "datetime?":
                    return DbType.DateTime;
                case "decimal":
                case "decimal?":
                    return DbType.Decimal;
                case "double":
                case "double?":
                case "float":
                case "float?":
                    return DbType.Double;
                case "int":
                case "int?":
                case "int32":
                case "int32?":
                    return DbType.Int32;
                case "int16":
                case "int16?":
                    return DbType.Int16;
                case "real":
                    return DbType.Single;
                case "guid":
                    return DbType.Guid;
                case "nullable`1":
                    if (type == typeof(Nullable<int>))
                    {
                        /* || type == typeof(Nullable<Int16>) || type == typeof(Nullable<Int32>))*/
                        return DbType.Int32;
                    }
                    else if (type == typeof(Nullable<long>))
                    {
                        /*|| type == typeof(Nullable<Int64>))*/
                        return DbType.Int64;
                    }
                    else if (type == typeof(Nullable<DateTime>))
                    {
                        return DbType.DateTime;
                    }
                    else if (type == typeof(Nullable<bool>))
                    {
                        /* || type == typeof(Nullable<Boolean>))*/
                        return DbType.Boolean;
                    }
                    else if (type == typeof(Nullable<decimal>))
                    {
                        /* || type == typeof(Nullable<Decimal>))*/
                        return DbType.Decimal;
                    }
                    else if (type == typeof(Nullable<float>) || type == typeof(Nullable<double>))
                    {
                        /* || type == typeof(Nullable<Double>))*/
                        return DbType.Double;
                    }
                    else if (type == typeof(Nullable<System.TimeSpan>))
                    {
                        return DbType.Time;
                    }
                    else
                    {
                        return DbType.String;
                    }
            }

            return DbType.String;
        }
    }
}
