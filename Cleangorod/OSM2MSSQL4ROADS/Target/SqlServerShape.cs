using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsmMsSqlUpload.Target
{
    static class SqlServerShape
    {
        // Result of select geography::STGeomFromText('POLYGON EMPTY', 4326)
        static readonly byte[] EmptyShape = new byte[]{
            0xE6, 0x10, 0x00, 0x00, 
            0x01, 0x04, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x01, 0x00, 0x00, 0x00, 
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
            0x03
        };
        static readonly byte[] ShapeHeader = new byte[] { 
            0xE6, 0x10, 0x00, 0x00, //> SRID
            0x01,                   //> Version 1
            0x04                    //> Flags Valid
        };
        static readonly byte[] FigShapeTrailer = new byte[]{
            0x01, 0x00, 0x00, 0x00,	//Nb Figures (1)
                0x01, 		            // - 01 Fig is stroke
                0x00, 0x00, 0x00, 0x00,	// - Offset = 0
            0x01, 0x00, 0x00, 0x00,	//Nb Shapes (1)
                0xFF, 0xFF, 0xFF, 0xFF,	// - Shape parent -> None
                0x00, 0x00, 0x00, 0x00,	// - Fig Offset -> first
                0x02		            // - GIS type -> LineString
        };
        static readonly byte[] FigPoint = new byte[]{
            0xE6, 0x10, 0x00, 0x00, //> SRID
            0x01,                   //> Version 1
            0x0C,                   //> Flags Valid+Point
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Lat
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00  // Lon
        };



        /// <summary>
        /// Create a SQL Server binary geography.
        /// Based on MS Spec at http://msdn.microsoft.com/en-us/library/ee320675(v=sql.105).aspx
        /// </summary>
        /// <param name="lineStrip">Line strip to convert</param>
        /// <returns>MS SQL Geography binary</returns>
        static public byte[] LineStringToByte(List<KeyValuePair<double, double>> lineStrip)
        {
            switch(lineStrip.Count)
            {
                case 0:
                    return (byte[])EmptyShape.Clone();
                case 1:
                    throw new Exception("LineString must have more than one point!");
                case 2:
                    var ret = new byte[38];
                    Array.Copy(ShapeHeader, 0, ret, 0, ShapeHeader.Length);
                    ret[5]=0x14; // Properties Valid+Line
                    Array.Copy(BitConverter.GetBytes(lineStrip[0].Key), 0, ret, 6, 8);
                    Array.Copy(BitConverter.GetBytes(lineStrip[0].Value), 0, ret, 14, 8);
                    Array.Copy(BitConverter.GetBytes(lineStrip[1].Key), 0, ret, 22, 8);
                    Array.Copy(BitConverter.GetBytes(lineStrip[1].Value), 0, ret, 30, 8);
                    return ret;
            }

            var length = 
                ShapeHeader.Length +
                4                       + //<- Nb Points
                16*lineStrip.Count;       //<- Each points
            var fig = new byte[length+FigShapeTrailer.Length];
            Array.Copy(ShapeHeader, 0, fig, 0, ShapeHeader.Length);
            Array.Copy(BitConverter.GetBytes(lineStrip.Count), 0, fig, 6, 4);
            for (var i = 0; i < lineStrip.Count; ++i)
            {
                var coord = lineStrip[i];
                Array.Copy(BitConverter.GetBytes(coord.Key), 0, fig, 10+(i*16), 8);
                Array.Copy(BitConverter.GetBytes(coord.Value),   0, fig, 18+(i*16), 8);
            }
            Array.Copy(FigShapeTrailer, 0, fig, length, FigShapeTrailer.Length);

            return fig;
        }

        static public byte[] ParsePoint(double lat, double lon)
        {
            var pts = (byte[])FigPoint.Clone();
            Array.Copy(BitConverter.GetBytes(lat), 0, pts, 6, 8);
            Array.Copy(BitConverter.GetBytes(lon), 0, pts, 14, 8);
            return pts;
        }
    }
}
