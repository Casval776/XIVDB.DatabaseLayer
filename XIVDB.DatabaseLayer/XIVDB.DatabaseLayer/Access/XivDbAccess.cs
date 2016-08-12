using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using XIVDB.DatabaseLayer.Global;
using XIVDB.DatabaseLayer.Helper;
using XIVDB.DatabaseLayer.Static;
using XIVDB.Interfaces;

namespace XIVDB.DatabaseLayer.Access
{
    /// <summary>
    /// The access point between the application and the database.
    /// Singleton instance that handles any and all calls to the Database.
    /// </summary>
    internal class XivDbAccess
    {
        #region Private Members
        private static readonly Lazy<XivDbAccess> Instance = new Lazy<XivDbAccess>(() => new XivDbAccess());
        private readonly Logger _log;
        private readonly SQLiteConnection _conn;
        #endregion

        #region Properties
        public static DbStatus Status { get; set; }
        #endregion

        #region Constructors

        private XivDbAccess()
        {
            //Create logger
            _log = new Logger(this);
            //Instantiate connection with DB
            _conn = new SQLiteConnection(Data.Database.ConnectionString);
        }

        static XivDbAccess() { }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns Instance of this object.
        /// Also makes necessary prerequisite checks on Database.
        /// </summary>
        /// <returns>XivDbAccess()</returns>
        internal static XivDbAccess GetInstance()
        {
            //Run necessary Database checks
            if (System.IO.File.Exists(Data.Database.FilePath)) return Instance.Value;
            SQLiteConnection.CreateFile(Data.Database.FilePath);
            Status = DbStatus.TablesNotCreated;
            //Return instance
            return Instance.Value;
        }

        /// <summary>
        /// Queries the database based on a model object
        /// </summary>
        /// <typeparam name="T">Type of object where T = IXivdbObject</typeparam>
        /// <param name="model">IXivdbObject with properties used to build query</param>
        /// <returns>IEnumerable of returned objects from the database</returns>
        internal IEnumerable<T> Get<T>(IXivdbObject model) where T : IXivdbObject
        {
            //Initialize return value
            IList<T> rtnList = new List<T>();
            //Deserialize the model object into SQL parameters
            var result = DatabaseHelper.Deserialize(model);
            //Use results to query database
            try
            {
                using (var cmd = _conn.CreateCommand())
                {
                    //Set command properties
                    cmd.CommandText = result;
                    cmd.CommandType = CommandType.Text;

                    //Execute query
                    _conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        //While objects are available to read...
                        while (reader.Read())
                        {
                            //Create object of Type T
                            var rtnObj = Activator.CreateInstance<T>();
                            //Serialize object and add to list
                            foreach (var prop in rtnObj.GetType().GetProperties())
                            {
                                prop.SetValue(rtnObj, reader[prop.Name]);
                            }
                            rtnList.Add(rtnObj);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                //Log exception and continue
                _log.Error($"Error occurred when querying the database on model item [{model.GetType().Name}]");
                ExceptionHandler.HandleException(exc);
            }
            finally
            {
                //Close connection
                if (_conn.State == ConnectionState.Open) _conn.Close();
            }
            return rtnList;
        }

        /// <summary>
        /// Inserts new record into the database
        /// </summary>
        /// <typeparam name="T">Type of object where T = IXivdbObject</typeparam>
        /// <param name="model">IXivdbObject with properties set to insert values</param>
        /// <returns>true or false</returns>
        public bool Insert<T>(IXivdbObject model) where T : IXivdbObject
        {
            //Use the helper to create the Insert string
            var insertString = DatabaseHelper.CreateInsert(model);
            try
            {
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = insertString;
                    cmd.CommandType = CommandType.Text;

                    _conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception exc)
            {
                _log.Error($"Error occurred while inserting record on model item [{model.GetType().Name}]");
                ExceptionHandler.HandleException(exc);
            }
            finally
            {
                _log.Info($"Inserted new [{model.GetType().Name}] with primary key [Id - {model.Id}]");
                if (_conn.State == ConnectionState.Open) _conn.Close();
            }
            return false;
        }
        #endregion

        #region Private Methods
        #endregion

        #region Internal Methods
        /// <summary>
        /// Creates tables in the database
        /// </summary>
        /// <param name="t">Type of object. Tables are created based on properties</param>
        /// <returns>true or false</returns>
        internal bool CreateTable(Type t)
        {
            try
            {
                //Copy template into a local variable
                var template = new StringBuilder(Data.Database.SQL.CreateTableTemplate);
                //Obtain a list of properties for the specified type
                var columnList = Activator.CreateInstance(t).GetType().GetProperties().Select(property =>
                    property.Name + " " +
                    DatabaseHelper.DataType[property.PropertyType] +
                    (property.Name == "Id" ? " PRIMARY KEY" : string.Empty)).ToList();
                //Replace template placeholders with actual data
                template = template.Replace("{NAME}", t.Name);
                template = template.Replace("{COLUMNS}", string.Join(",", columnList));
                //Create table
                using (var cmd = _conn.CreateCommand())
                {
                    //Set Command properties
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = template.ToString();
                    //Open connection
                    _conn.Open();
                    //Execute query
                    cmd.ExecuteNonQuery();
                    _log.Info("Created table [" + t.Name + "]");
                    return true;
                }
            }
            catch (Exception exc)
            {
                //Handle exception and return fail
                ExceptionHandler.HandleException(exc);
                return false;
            }
            finally
            {
                //Close connection
                if (_conn.State == ConnectionState.Open) _conn.Close();
            }
        }
        #endregion
    }
}
