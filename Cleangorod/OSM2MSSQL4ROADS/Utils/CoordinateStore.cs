using System.Collections.Generic;

namespace OsmMsSqlUpload.Utils
{
    /// <summary>
    /// Lossless conversion of OSM coordinates to a simple long.
    /// </summary>
    unsafe class CoordinateStore
    {
        private readonly double _lat, _lon;
        private readonly long _encoded;

        public CoordinateStore(double lon,double lat)
        {
            // Ensure valid lat/lon
            if (lon < -180.0) lon = 180.0+(lon+180.0); else if (lon > 180.0) lon = -180.0 + (lon-180.0);
            if (lat < -90.0) lat = 90.0 + (lat + 90.0); else if (lat > 90.0) lat = -90.0 + (lat - 90.0);

            _lon = lon; _lat = lat;

            // Move to 0..(180/90)
            var dlon = (decimal)lon + 180m;
            var dlat = (decimal)lat + 90m;

            // Calculate grid
            var grid = (((int)dlat) * 360) + ((int)dlon);

            // Get local offset
            var ilon = (uint)((dlon - (int)(dlon))*10000000m);
            var ilat = (uint)((dlat - (int)(dlat))*10000000m);

            var encoded = new byte[8];
            fixed (byte* pEncoded = &encoded[0])
            {
                ((ushort*)pEncoded)[0] = (ushort) grid;
                ((ushort*)pEncoded)[1] = (ushort)(ilon&0xFFFF);
                ((ushort*)pEncoded)[2] = (ushort)(ilat&0xFFFF);
                pEncoded[6] = (byte)((ilon >> 16)&0xFF);
                pEncoded[7] = (byte)((ilat >> 16)&0xFF);

                _encoded = ((long*) pEncoded)[0];
            }
        }

        public CoordinateStore(long source)
        {
            // Extract grid and local offset
            int grid;
            decimal ilon, ilat;
            var encoded = new byte[8];
            fixed(byte *pEncoded = &encoded[0])
            {
                ((long*) pEncoded)[0] = source;
                grid = ((ushort*) pEncoded)[0];
                ilon = ((ushort*)pEncoded)[1] + (((uint)pEncoded[6]) << 16);
                ilat = ((ushort*)pEncoded)[2] + (((uint)pEncoded[7]) << 16);
            }

            // Recalculate 0..(180/90) coordinates
            var dlon = (uint)(grid % 360) + (ilon / 10000000m);
            var dlat = (uint)(grid / 360) + (ilat / 10000000m);

            // Returns to WGS84
            _lon = (double)(dlon - 180m);
            _lat = (double)(dlat - 90m);
        }

        public double Lon { get { return _lon; } }
        public double Lat { get { return _lat; } }
        public long   Encoded { get { return _encoded; } }


        public static long PackCoord(double lon,double lat)
        {
            return (new CoordinateStore(lon, lat)).Encoded;
        }
        public static KeyValuePair<double, double> UnPackCoord(long coord)
        {
            var tmp = new CoordinateStore(coord);
            return new KeyValuePair<double, double>(tmp.Lat,tmp.Lon);
        }
    }
}
