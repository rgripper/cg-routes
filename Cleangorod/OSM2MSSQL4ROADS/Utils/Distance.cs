using System;
using System.Collections.Generic;

namespace OsmMsSqlUpload.Utils
{
    static class Distance
    {
        /// <summary>  
        /// Returns the distance in kilometers of any two latitude / longitude points.  
        /// </summary>  
        static public double Calc(double pos1X, double pos1Y, double pos2X, double pos2Y)
        {
            const double R = 6371;
            var dLat = ToRadian(pos2Y - pos1Y);
            var dLon = ToRadian(pos2X - pos1X);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadian(pos1Y)) * Math.Cos(ToRadian(pos2Y)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            var d = R * c;
            return d;
        }

        static double ToRadian(double val)
        {
            return (Math.PI / 180) * val;
        }

        /// <summary>
        /// Calculate the distance of a LineString
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        static public double Calc(List<KeyValuePair<double,double>> list)
        {
            double dist = 0;

            for(var i=1;i<list.Count;++i)
            {
                dist += Calc(list[i - 1].Key, list[i - 1].Value,
                             list[i    ].Key, list[i    ].Value);
            }

            return dist;
        }

    }
}