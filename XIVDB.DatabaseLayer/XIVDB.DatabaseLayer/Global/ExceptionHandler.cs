using System;

namespace XIVDB.DatabaseLayer.Global
{
    internal static class ExceptionHandler
    {
        private static readonly Logger Log = new Logger(typeof(ExceptionHandler));
        public static void HandleException(Exception exc)
        {
            //For now, just log it.
            Log.Fatal(exc.Message + Environment.NewLine + exc.StackTrace);
        }
    }
}
