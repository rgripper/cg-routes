using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using OsmMsSqlUpload.Target;

namespace OsmMsSqlUpload.Analyser
{
    interface ITempDatabase : IDisposable
    {
        void InsertNode     (long nid, double lat, double lon);
        void InsertWay      (long wid, int type, bool oneWay);
        void InsertWayPoints(long wid, IEnumerable<long> nids);

        void DetectIntersections(ServerUploadBase upload, IAnalyserProgress progress);

        IDataReader GetWays();
    }
}
