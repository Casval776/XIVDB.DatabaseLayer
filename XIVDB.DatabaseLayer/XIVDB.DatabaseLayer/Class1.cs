using System.Linq;
using System.Runtime.InteropServices;
using XIVDB.DatabaseLayer.Client;
using XIVDB.DatabaseLayer.Global;
using XIVDB.Model;

namespace XIVDB.DatabaseLayer
{
    public class Class1
    {
        public Class1()
        {
            //XmlConfigurator.Configure();
            var log = new Logger(this);
            log.Warning("Debug");
        }
        public static void Main(string[] args)
        {
            var debug = new Class1();
            var debugObject = new Item();
            //var something = debugObject.GetType().GetProperties().Where(p => p.)
            var client = DbClient.GetInstance();
            var returnList = client.Get<Item>(new Item() {Action = 5});
            var returnVal = client.Insert<Item>(new Item(){Id = 44, Action = 5});
            var client2 = DbClient.GetInstance();
        }
    }
}
