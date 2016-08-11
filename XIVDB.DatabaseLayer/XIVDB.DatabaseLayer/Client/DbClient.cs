﻿using System;
using System.Collections.Generic;
using System.Linq;
using XIVDB.DatabaseLayer.Service;
using XIVDB.Interfaces;

namespace XIVDB.DatabaseLayer.Client
{
    /// <summary>
    /// Endpoint for external applications.
    /// All XIVDB.Database related functionality comes through this client.
    /// </summary>
    public class DbClient
    {
        #region Private Members
        private static readonly Lazy<DbClient> Instance = new Lazy<DbClient>(() => new DbClient());
        private static MainService _service;
        #endregion

        #region Constructors
        private DbClient()
        {
            //Initialize necessary members
            _service = new MainService();
        }

        static DbClient()
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns lazy-loaded instance of this object
        /// </summary>
        /// <returns>DbClient()</returns>
        public static DbClient GetInstance() { return Instance.Value; }

        /// <summary>
        /// Returns an IEnumerable of database results
        /// </summary>
        /// <typeparam name="T">Type of object where T = IXivdbObject</typeparam>
        /// <param name="model">IXivdbObject with properties used to construct query</param>
        /// <returns>IEnumerable of results (non-nullable)</returns>
        public IEnumerable<T> Get<T>(IXivdbObject model) where T : IXivdbObject
        {
            return _service.Get<T>(model);
        }

        /// <summary>
        /// Returns the first or default value of the returned results from the DB
        /// </summary>
        /// <typeparam name="T">Type of object where T = IXivdbObject</typeparam>
        /// <param name="model">IXivdbObject with properties used to construct query</param>
        /// <returns>IXivdbObject or default value</returns>
        public IXivdbObject GetSingle<T>(IXivdbObject model) where T : IXivdbObject
        {
            return _service.Get<T>(model).FirstOrDefault();
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
