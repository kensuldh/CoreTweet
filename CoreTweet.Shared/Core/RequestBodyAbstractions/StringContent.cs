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
using System.Text;

#if !NET35
using System.Threading;
using System.Threading.Tasks;
#endif

namespace CoreTweet.Core.RequestBodyAbstractions
{
    public abstract class StringContent :
#if NET35
        IRequestContentWriter
#else
#if !ASYNC_ONLY
        IRequestContentWriter,
#endif
        IAsyncRequestContentWriter
#endif
    {
        public string ContentString { get; }
        public byte[] ContentBytes { get; }

        public StringContent(string content)
        {
            this.ContentString = content;
            this.ContentBytes = Encoding.UTF8.GetBytes(content);
        }

        public abstract string ContentType { get; }

        private readonly static KeyValuePair<string, string>[] contentTypeParameters = { new KeyValuePair<string, string>("charset", "utf-8") };
        public virtual IEnumerable<KeyValuePair<string, string>> ContentTypeParameters => contentTypeParameters;

        public long? ContentLength => this.ContentBytes.Length;

#if !ASYNC_ONLY
        public void WriteTo(Writer writer)
        {
            writer(this.ContentBytes, 0, this.ContentBytes.Length);
        }
#endif

#if !NET35
        public Task WriteToAsync(AsyncWriter writer, CancellationToken cancellationToken, UploadProgressReporter reporter)
        {
            var task = writer(this.ContentBytes, 0, this.ContentBytes.Length, cancellationToken, reporter == null ? null :
                new WriteProgressReporter(bytesWritten => reporter(new UploadProgressInfo(bytesWritten, this.ContentBytes.Length))));
            return reporter == null ? task :
                task.Done(() => reporter(new UploadProgressInfo(this.ContentBytes.Length, this.ContentBytes.Length)), cancellationToken);
        }
#endif
    }
}
