using System;
using System.Linq;
using System.Windows.Forms;

namespace OsmMsSqlUpload
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Enumerable.Range(8, 23 - 8).Where(x => x % 3 == 0).ToList().ForEach(x => Console.WriteLine("{0} {1}", x, x + 2));
            Console.ReadLine();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new OSM2MSSQL4ROADS());
            var loader = new Loader();
            loader.Load(@"C:\Users\cgfqh_000\Downloads\moscow.osm.pbf", "OSM");
        }
    }
}
