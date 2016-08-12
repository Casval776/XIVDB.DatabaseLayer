using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using XIVDB.DatabaseLayer.Static;
using XIVDB.Interfaces;

namespace XIVDB.DatabaseLayer.Helper
{
    /// <summary>
    /// Helper class for Database operations.
    /// Contains data and functions to simplify and promote maintainability.
    /// </summary>
    internal static class DatabaseHelper
    {
        /// <summary>
        /// Static Dictionary to assist with .Net -> SQL data type mapping
        /// </summary>
        public static readonly Dictionary<Type, string> DataType = new Dictionary<Type, string>()
        {
            { typeof(int?), "integer" },
            { typeof(string), "varchar" },
            { typeof(bool?), "integer" },
            { typeof(DateTime?), "text" }
        };

        /// <summary>
        /// Deserializes an IXivdbObject into a Select query
        /// </summary>
        /// <param name="model">Model object with properties used to build the query</param>
        /// <returns>Select statement with deserialized properties</returns>
        public static string Deserialize(IXivdbObject model)
        {
            var template = new StringBuilder(Data.Database.SQL.SelectTemplate);
            //Loop through object to get non-null properties
            foreach (var property in model.GetType().GetProperties().Where(property => property.GetValue(model) != null))
            {
                //If property is string or DateTime, surround comparison value in single quotes
                if (property.PropertyType == typeof (string) ||
                    property.PropertyType == typeof(DateTime))
                    template.AppendLine(" AND " + property.Name + " = '" + Convert.ToString(property.GetValue(model)) + "'");
                else
                    template.AppendLine(" AND " + property.Name + " = " + Convert.ToString(property.GetValue(model)));
            }
            //Set table name and return
            return template.Replace("{TABLE}", model.GetType().Name).ToString();
        }

        public static string CreateInsert(IXivdbObject model)
        {
            var template = new StringBuilder(Data.Database.SQL.InsertTemplate);
            var valList = model.GetType().GetProperties()
                .Select(property => property.GetValue(model))
                .Select(objVal => objVal == null 
                    ? "null" 
                    : Convert.ToString(objVal))
                .ToList();
            return template.Replace("{NAME}", model.GetType().Name)
                .Replace("{VALUES}", string.Join(",", valList))
                .ToString();
        }
    }
}
