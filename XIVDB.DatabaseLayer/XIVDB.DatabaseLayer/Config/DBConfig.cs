using System;

namespace XIVDB.DatabaseLayer.Config
{
    /// <summary>
    /// Internal class to manage and check Database settings
    /// </summary>
    internal class DBConfig
    {
        #region Private Members
        private static readonly Lazy<DBConfig> _instance = new Lazy<DBConfig>(() => new DBConfig());
        #endregion

        #region Constructors
        private DBConfig() { }
        static DBConfig() { }
        #endregion

        #region Public Methods
        public DBConfig GetInstance() { return _instance.Value; }
        #endregion

        #region Private Methods
        #endregion
    }
}
