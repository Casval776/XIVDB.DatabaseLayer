namespace XIVDB.DatabaseLayer.Static
{
    internal struct Data
    {
        public const string EmptyString = "";
        public struct Database
        {
            public const string DatabaseName = "XIVDB.Database.sqlite";
            public const string ConnectionString = "DataSource=" + FilePath + ";Version=3";
            public const string FilePath = @"Database\" + DatabaseName;
            public const string DirectoryName = @"Database\";

            public struct Config
            {
                public const string FileName = "DBConfig.xml";
                // ReSharper disable once MemberHidesStaticFromOuterClass
                public const string FilePath = @"\Database\" + FileName;
            }

            public struct SQL
            {
                public const string SelectTemplate = @"SELECT * FROM {TABLE} WHERE 1 = 1";
                public const string CreateTableTemplate = @"CREATE TABLE {NAME} ( {COLUMNS} );";
            }
        }
        public struct App
        {
            public const string ModelNamespace = "XIVDB.Model";
            public const string DataLayerAssembly = "XIVDB";
        }
    }
}
