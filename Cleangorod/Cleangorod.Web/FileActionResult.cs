using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace Cleangorod.Web
{
    public class FileActionResult : IHttpActionResult
    {
        public FileActionResult(Stream stream, string fileName, string mediaType)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            _Stream = stream;
            _FileName = fileName;
            _MediaType = mediaType;
        }

        Stream _Stream;
        string _FileName;
        string _MediaType;

        static readonly Regex _InternetExplorerRegex = new Regex(@"(?:\b(MS)?IE\s+|\bTrident\/7\.0;.*\s+rv:)(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage();
            response.Content = new StreamContent(_Stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(_MediaType);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = _InternetExplorerRegex.IsMatch(HttpContext.Current.Request.UserAgent) ? Uri.EscapeDataString(_FileName) : _FileName
            };
            return Task.FromResult(response);
        }
    }
}