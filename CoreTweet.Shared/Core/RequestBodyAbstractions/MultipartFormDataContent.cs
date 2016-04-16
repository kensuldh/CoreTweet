// The MIT License (MIT)
//
// CoreTweet - A .NET Twitter Library supporting Twitter API 1.1
// Copyright (c) 2013-2016 CoreTweet Development Team
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

#if !NET35
using System.Threading;
using System.Threading.Tasks;
#endif

namespace CoreTweet.Core.RequestBodyAbstractions
{
    public abstract class PlatformIndependentMultipartFormDataContent : IContentInfo
    {
        protected static byte[] NewLineBytes { get; } = { 0x0D, 0x0A };

        public long? ContentLength { get; private set; }
        public string ContentType => "multipart/form-data";
        public IEnumerable<KeyValuePair<string, string>> ContentTypeParameters { get; }
        protected string Boundary { get; }
        protected byte[] LastLineBytes { get; }
        protected byte[][] ItemHeadersBytes { get; private set; }

        protected PlatformIndependentMultipartFormDataContent(IMultipartFormDataItemInfo[] items)
        {
            this.Boundary = Guid.NewGuid().ToString();
            this.ContentTypeParameters = new[] { new KeyValuePair<string, string>("boundary", this.Boundary) };
            this.LastLineBytes = Encoding.UTF8.GetBytes("--" + this.Boundary + "--");
            CreateItemHeadersBytes(items);
            ComputeContentLength(items);
        }

        private void CreateItemHeadersBytes(IMultipartFormDataItemInfo[] items)
        {
            this.ItemHeadersBytes = new byte[items.Length][];
            var sb = new StringBuilder();
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                sb.Length = 0;
                sb.Append("--").Append(this.Boundary).Append("\r\n");
                if (!string.IsNullOrEmpty(item.ContentType))
                    sb.Append("Content-Type: ").Append(item.ContentType).Append("\r\n");
                sb.Append("Content-Disposition: form-data; name=\"").Append(item.Name).Append('"');
                if (!string.IsNullOrEmpty(item.FileName))
                    sb.Append("; filename=\"")
                        .Append(item.FileName.Replace("\n", "%0A").Replace("\r", "%0D").Replace("\"", "%22"))
                        .Append('"');
                sb.Append("\r\n\r\n");
                this.ItemHeadersBytes[i] = Encoding.UTF8.GetBytes(sb.ToString());
            }
        }

        private void ComputeContentLength(IMultipartFormDataItemInfo[] items)
        {
            long contentLength = 0;
            foreach (var item in items)
            {
                if (!item.ContentLength.HasValue) return;
                contentLength += item.ContentLength.Value;
            }
            foreach (var x in this.ItemHeadersBytes)
                contentLength += x.Length;
            contentLength += NewLineBytes.Length * items.Length;
            contentLength += this.LastLineBytes.Length;
            this.ContentLength = contentLength;
        }
    }

#if !ASYNC_ONLY
    public class MultipartFormDataContent : PlatformIndependentMultipartFormDataContent, IContentWriter
    {
        private readonly IMultipartItemContentWriter[] items;

        private MultipartFormDataContent(IMultipartItemContentWriter[] items)
            : base(items)
        {
            this.items = items;
        }

        public MultipartFormDataContent(IEnumerable<IMultipartItemContentWriter> items)
            : this(items.ToArray()) { }

        public void WriteTo(Writer writer)
        {
            for (var i = 0; i < this.items.Length; i++)
            {
                var header = this.ItemHeadersBytes[i];
                writer(header, 0, header.Length);
                this.items[i].WriteTo(writer);
                writer(NewLineBytes, 0, NewLineBytes.Length);
            }
            writer(this.LastLineBytes, 0, this.LastLineBytes.Length);
        }
    }
#endif

#if !NET35
    public class MultipartFormDataContentAsync : PlatformIndependentMultipartFormDataContent, IAsyncContentWriter
    {
        private readonly IAsyncMultipartItemContentWriter[] items;

        private MultipartFormDataContentAsync(IAsyncMultipartItemContentWriter[] items)
            : base(items)
        {
            this.items = items;
        }

        public MultipartFormDataContentAsync(IEnumerable<IAsyncMultipartItemContentWriter> items)
            : this(items.ToArray()) { }

        public Task WriteToAsync(AsyncWriter writer, CancellationToken cancellationToken, UploadProgressReporter reporter)
        {
            var i = 0;
            long bytesWritten = 0;
            var lastReport = 0;
            var r = reporter == null ? null : new WriteProgressReporter(x =>
            {
                lastReport = x;
                reporter(new UploadProgressInfo(bytesWritten + x, this.ContentLength));
            });
            Func<Task> action = null;
            action = () =>
            {
                if (i >= this.items.Length)
                    return InternalUtils.CompletedTask;

                var header = this.ItemHeadersBytes[i];
                return writer(header, 0, header.Length, cancellationToken, r)
                    .Done(() =>
                    {
                        bytesWritten += header.Length;
                        return this.items[i].WriteToAsync(writer, cancellationToken, r);
                    }, cancellationToken)
                    .Unwrap()
                    .Done(() =>
                    {
                        bytesWritten += this.items[i].ContentLength ?? lastReport;
                        return writer(NewLineBytes, 0, NewLineBytes.Length, cancellationToken, r);
                    }, cancellationToken)
                    .Unwrap()
                    .Done(() =>
                    {
                        bytesWritten += NewLineBytes.Length;
                        i++;
                        return action();
                    }, cancellationToken)
                    .Unwrap();
            };
            var task = action()
                .Done(() => writer(this.LastLineBytes, 0, this.LastLineBytes.Length, cancellationToken, r), cancellationToken)
                .Unwrap();
            return reporter == null ? task : task.Done(() =>
            {
                Debug.Assert(!this.ContentLength.HasValue || this.ContentLength.Value == bytesWritten + this.LastLineBytes.Length);
                reporter(new UploadProgressInfo(bytesWritten + this.LastLineBytes.Length, this.ContentLength ?? (bytesWritten + this.LastLineBytes.Length)));
            }, cancellationToken);
        }
    }
#endif
}
