using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsmMsSqlUpload.Analyser
{
    interface IAnalyserProgress
    {
        void OnAnalyserProgress (int percentage);
        void OnAnalyserStep     (string text);

        void OnIntersectionNb   (long nbIntersection);
        void OnTotalEdges       (long nbEdges);
    }
}
