using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XIVDB.DatabaseLayer.Access;
using XIVDB.DatabaseLayer.Global;
using XIVDB.DatabaseLayer.Static;
using XIVDB.Interfaces;

namespace XIVDB.DatabaseLayer.Service
{
    /// <summary>
    /// Service class used to interact with XIV Database
    /// </summary>
    internal class MainService
    {
        #region Private Members
        private static XivDbAccess _access;
        private readonly Logger _log;
        #endregion

        #region Constructors
        public MainService()
        {
            //Instantiate logger
            _log = new Logger(this);
            if (_access == null) _access = XivDbAccess.GetInstance();
            //Validate database
            ValidateDbStatus();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns an IEnumerable of results returned from the database
        /// </summary>
        /// <typeparam name="T">Type of object where T = IXivdbObject</typeparam>
        /// <param name="model">IXivdbObject with properties used to construct query</param>
        /// <returns>IEnumerable of IXivdbObjects returned from database</returns>
        public IEnumerable<T> Get<T>(IXivdbObject model) where T : IXivdbObject
        {
            return _access.Get<T>(model);
        }

        /// <summary>
        /// Inserts new record to database. Primary Key cannot be null.
        /// </summary>
        /// <typeparam name="T">Type of object where T = IXivdbObject</typeparam>
        /// <param name="model">IXivdbObject with properties used as insert parameters</param>
        /// <returns>true or false</returns>
        public bool Insert<T>(IXivdbObject model) where T : IXivdbObject
        {
            //Inverted conditional
            if (model.Id != null) return _access.Insert<T>(model);
            //If primary key is null, do not insert.
            //All objects returned from the API have a primary key
            _log.Warning($"Insert attempted with null Primary Key on Model type [{model.GetType().Name}]");
            return false;
        }

        /// <summary>
        /// Updates record in database. Primary Key cannot be null.
        /// </summary>
        /// <typeparam name="T">Type of object where T = IXivdbObject</typeparam>
        /// <param name="model">IXivdbObject with properties used to update</param>
        /// <returns>true or false</returns>
        public bool Update<T>(IXivdbObject model) where T : IXivdbObject
        {
            //Inverted conditional
            if (model.Id != null) return _access.Update<T>(model);
            //If primary key is null, do not update.
            //We will not be performing updates for items that are not returned by the API
            _log.Warning($"Update attempted with null Primary Key on Model type [{model.GetType().Name}]");
            return false;
        }

        /// <summary>
        /// Inserts record in database if it doesn't already exist. Primary Key cannot be null.
        /// </summary>
        /// <typeparam name="T">Type of object where T = IXivdbObject</typeparam>
        /// <param name="model">IXivdbObject with properties used to insert</param>
        /// <returns>true or false</returns>
        public bool InsertIfNotExists<T>(IXivdbObject model) where T : IXivdbObject
        {
            //If primary key is null, don't bother checking anything else
            if (model.Id != null) return !_access.Get<T>(model).Any() && _access.Insert<T>(model);
            _log.Warning($"InsertIfNotExists attempted with null Primary Key on Model type [{model.GetType().Name}]");
            return false;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Reads Status property from XivDbAccess class to determine what to do
        /// </summary>
        private void ValidateDbStatus()
        {
            //Check dbStatus and take appropriate action
            switch (XivDbAccess.Status)
            {
                case DbStatus.TablesNotCreated: CreateDbTables();
                    break;
                case DbStatus.DatabaseFileNotFound: _log.Fatal("Fatal Error occured. Database File not found.");
                    break;
                case DbStatus.DatabaseConfigFileNotFound: _log.Info("DatabaseConfigFileNotFound status detected.");
                    break;
                case DbStatus.Ok: _log.Info("Database Status is OK.");
                    break;
                case DbStatus.Unknown: _log.Warning("Database Status is unknown...");
                    break;
                default:
                    _log.Warning("Unknown DbStatus value in switch statement...");
                    break;
            }
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Scans the Assembly for the Model namespace to create DB tables
        /// </summary>
        internal void CreateDbTables()
        {
            //Foreach Model Type found in the Model namespace...
            foreach (var model 
                in Assembly.Load(Data.App.DataLayerAssembly).GetTypes()
                .Where(t => t.IsClass && t.Namespace == Data.App.ModelNamespace))
            {
                //Pass Type to XivDbAccess class
                try
                {
                    //If returned result is failure, log it and continue
                    if (!_access.CreateTable(model)) _log.Warning($"Error occured during table creation for object [{model.Name}].");
                }
                catch (Exception exc)
                {
                    ExceptionHandler.HandleException(exc);
                    _log.Fatal("Fatal error occured during processing model classes...");
                }
            }
            //Finally, update dbstatus
            XivDbAccess.Status = DbStatus.Ok;
        }
        #endregion
    }
}
