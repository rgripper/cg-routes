using System;
using System.IO;
using System.IO.Compression;
using OsmMsSqlUpload.Source.PbfBuffers;
using ProtoBuf;

namespace OsmMsSqlUpload.Source
{
    class PBFReader : IDisposable
    {
        public const int MaxDataBlockSize = 32 * 1024 * 1024;
        public const int MaxHeaderBlockSize = 64 * 1024;

        readonly System.IO.Stream _file;
        readonly OSMConsumer _backend;

        readonly OnReaderProgress _progress;

        public PBFReader(System.IO.Stream file, OSMConsumer backend, OnReaderProgress progress)
        {
            _file = file;
            _backend = backend;
            _progress = progress;
        }

        public void Dispose()
        {
        }

        BlobHeader ReadBlobHeader()
        {
            return _file.Position < _file.Length ?
                Serializer.DeserializeWithLengthPrefix<BlobHeader>(_file, PrefixStyle.Fixed32BigEndian) : 
                null;
        }

        object ReadBlob(BlobHeader header)
        {
            var buffer = new byte[header.DataSize];
            _file.Read(buffer, 0, header.DataSize);
            Blob blob;
            using (var s = new MemoryStream(buffer))
            {
                blob = Serializer.Deserialize<Blob>(s);
            }

            Stream blobContentStream = null;
            try
            {
                if (blob.Raw != null)
                {
                    blobContentStream = new MemoryStream(blob.Raw);
                }
                else if (blob.ZlibData != null)
                {
                    var deflateStreamData = new MemoryStream(blob.ZlibData);
                    //skip ZLIB header
                    deflateStreamData.Seek(2, SeekOrigin.Begin);
                    blobContentStream = new DeflateStream(deflateStreamData, CompressionMode.Decompress);
                }

                if (header.Type.Equals("OSMData", StringComparison.InvariantCultureIgnoreCase))
                {
                    if ((blob.RawSize.HasValue && blob.RawSize > MaxDataBlockSize) ||
                        (blob.RawSize.HasValue == false && blobContentStream.Length > MaxDataBlockSize))
                    {
                        throw new InvalidDataException("Invalid OSMData block");
                    }

                    return Serializer.Deserialize<PrimitiveBlock>(blobContentStream);
                }
                else if (header.Type.Equals("OSMHeader", StringComparison.InvariantCultureIgnoreCase))
                {
                    if ((blob.RawSize.HasValue && blob.RawSize > MaxHeaderBlockSize) ||
                        (blob.RawSize.HasValue == false && blobContentStream.Length > MaxHeaderBlockSize))
                    {
                        throw new InvalidDataException("Invalid OSMHeader block");
                    }

                    return Serializer.Deserialize<OsmHeader>(blobContentStream);
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                if (blobContentStream != null)
                {
                    blobContentStream.Close();
                    blobContentStream.Dispose();
                    blobContentStream = null;
                }
            }
        }


        void ProcessNodes(PrimitiveBlock block, PrimitiveGroup group)
        {
            if (group.Nodes == null) return;

            foreach (PbfNode node in group.Nodes)
            {
                double lat = 1E-09 * (block.LatOffset + (block.Granularity * node.Latitude));
                double lon = 1E-09 * (block.LonOffset + (block.Granularity * node.Longitude));

                ++TotalNodes;
                _backend.OnNode(node.ID, lat, lon);
            }
        }

        void ProcessDenseNodes(PrimitiveBlock block, PrimitiveGroup group)
        {
            if (group.DenseNodes == null) return;

            long idStore = 0;
            long latStore = 0;
            long lonStore = 0;

            for (var i = 0; i < group.DenseNodes.Id.Count; i++)
            {
                idStore += group.DenseNodes.Id[i];
                lonStore += group.DenseNodes.Longitude[i];
                latStore += group.DenseNodes.Latitude[i];

                var lat = 1E-09 * (block.LatOffset + (block.Granularity * latStore));
                var lon = 1E-09 * (block.LonOffset + (block.Granularity * lonStore));

                ++TotalNodes;
                _backend.OnNode(idStore,lat,lon);
            }
        }

        void ProcessWays(PrimitiveBlock block, PrimitiveGroup group)
        {
            if (group.Ways == null) return;

            foreach (var way in group.Ways)
            {
                ++TotalWays;
                _backend.OnWay(way.ID.ToString());

                if (way.Keys != null)
                {
                    for (int i = 0; i < way.Keys.Count; i++)
                    {
                        _backend.OnTag(
                            block.StringTable[way.Keys[i]],
                            block.StringTable[way.Values[i]]
                        );
                    }
                }
                long refStore = 0;
                foreach (var t in way.Refs)
                {
                    refStore += t;
                    _backend.OnNd(refStore);
                }
            }
        }

        void ProcessRelations(PrimitiveBlock block, PrimitiveGroup group)
        {
            if (group.Relations == null) return;

            foreach (var relation in group.Relations)
            {
                ++TotalRelations;
                _backend.OnRelation(relation.ID.ToString());
                if (relation.Keys != null)
                {
                    for (int i = 0; i < relation.Keys.Count; i++)
                    {
                        _backend.OnTag(
                            block.StringTable[relation.Keys[i]],
                            block.StringTable[relation.Values[i]]
                        );
                    }
                }
                long refStore = 0;

                for (var i = 0; i < relation.MemberIds.Count; i++)
                {
                    refStore += relation.MemberIds[i];

                    _backend.OnMember(
                        relation.Types[i],
                        refStore,
                        block.StringTable[relation.RolesIndexes[i]]
                        );
                }
            }
        }

        public long TotalNodes { get; private set; }
        public long TotalWays { get; private set; }
        public long TotalRelations { get; private set; }

        public void Process()
        {
            var size = _file.Length;
            var increment = size / 100;
            var nextPosition = increment;
            var percent = 0;
            var lastRefresh = DateTime.Now;

            _progress(0, 0, 0);

            BlobHeader blobHeader = null;

            while ((blobHeader = ReadBlobHeader()) != null)
            {
                var block = ReadBlob(blobHeader) as PrimitiveBlock;
                if (block == null) continue;

                if (_file.Position > nextPosition)
                {
                    _progress(++percent, TotalNodes, TotalWays);
                    nextPosition += increment;
                    lastRefresh = DateTime.Now;
                }
                else
                {
                    var now = DateTime.Now;
                    if (now.Subtract(lastRefresh).Seconds > ProcessDlg.RefreshRate)
                    {
                        _progress(percent, TotalNodes, TotalWays);
                        lastRefresh = now;
                    }
                }

                foreach (PrimitiveGroup group in block.PrimitiveGroup)
                {
                    ProcessNodes        (block, group);
                    ProcessDenseNodes   (block, group);
                    ProcessWays         (block, group);
                    ProcessRelations    (block, group);
                }
            }

            _progress(100, TotalNodes, TotalWays);
        }

        public static void RunUpload(string sourceFile, OnReaderProgress progress, OSMConsumer backend)
        {
            try
            {
                using (var file = System.IO.File.OpenRead(sourceFile))
                using (var processor = new PBFReader(file, backend, progress))
                {
                    processor.Process();
                }
            }
            catch (System.IO.EndOfStreamException)
            {
                // Silently ignore this one.
            }
        }
    }
}
