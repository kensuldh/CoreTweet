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
using System.IO;
using System.Text;

#if !NET35
using System.Threading;
using System.Threading.Tasks;
#endif

namespace CoreTweet.Core.RequestBodyAbstractions
{
    internal abstract class MultipartItem :
#if NET35
        IMultipartItemContentWriter
#else
#if !ASYNC_ONLY
        IMultipartItemContentWriter,
#endif
        IAsyncMultipartItemContentWriter
#endif
    {
        public string Name { get; }

        public virtual string FileName => null;

        public virtual string ContentType => null;

        public abstract long? ContentLength { get; }

        protected MultipartItem(string name)
        {
            this.Name = name;
        }

#if !ASYNC_ONLY
        public abstract void WriteTo(Writer writer);
#endif

#if !NET35
        public abstract Task WriteToAsync(AsyncWriter writer, CancellationToken cancellationToken, WriteProgressReporter reporter);
#endif

        public static MultipartItem Create(string name, object value)
        {
            var valueStream = value as Stream;
            if (valueStream != null)
                return new StreamMultipartItem(name, valueStream);

            if (value is ArraySegment<byte>)
                return new ArraySegmentMultipartItem(name, (ArraySegment<byte>)value);

            var valueByteArray = value as byte[];
            if (valueByteArray != null)
                return new ByteArrayMultipartItem(name, valueByteArray);

            var valueBytes = value as IEnumerable<byte>;
            if (valueBytes != null)
                return new ByteEnumerableMultipartItem(name, valueBytes);

#if !(PCL || WIN_RT)
            var valueFile = value as FileInfo;
            if (valueFile != null)
                return new FileInfoMultipartItem(name, valueFile);
#endif

#if WP
            var valueInputStream = value as Windows.Storage.Streams.IInputStream;
            if (valueInputStream != null)
                return new StreamMultipartItem(name, valueInputStream.AsStreamForRead());
#endif

            //TODO: Windows Universal 専用のやつ

            return new StringMultipartItem(name, value.ToString());
        }
    }

    internal class StringMultipartItem : MultipartItem
    {
        public string Value { get; }

        private readonly byte[] valueBytes;

        public override long? ContentLength => valueBytes.Length;

        public StringMultipartItem(string name, string value)
            : base(name)
        {
            this.Value = value;
            this.valueBytes = Encoding.UTF8.GetBytes(value);
        }

#if !ASYNC_ONLY
        public override void WriteTo(Writer writer)
        {
            writer(this.valueBytes, 0, this.valueBytes.Length);
        }
#endif

#if !NET35
        public override Task WriteToAsync(AsyncWriter writer, CancellationToken cancellationToken, WriteProgressReporter reporter)
        {
            return writer(this.valueBytes, 0, this.valueBytes.Length, cancellationToken, reporter);
        }
#endif
    }

    internal abstract class FileMultipartItem : MultipartItem
    {
        protected const int bufferSize = 81920;

        protected FileMultipartItem(string name)
            : base(name) { }

        public override string ContentType => "application/octet-stream";

        public override string FileName => "file";
    }

    internal class StreamMultipartItem : FileMultipartItem
    {
        public Stream Content { get; }

        public StreamMultipartItem(string name, Stream content)
            : base(name)
        {
            this.Content = content;
        }

        public override long? ContentLength => this.Content.CanSeek ? (long?)this.Content.Length : null;

#if !ASYNC_ONLY
        public override void WriteTo(Writer writer)
        {
            var buffer = new byte[bufferSize];
            int count;
            while ((count = this.Content.Read(buffer, 0, bufferSize)) > 0)
                writer(buffer, 0, count);
        }
#endif

#if !NET35
        public override Task WriteToAsync(AsyncWriter writer, CancellationToken cancellationToken, WriteProgressReporter reporter)
        {
            int bytesWritten = 0;
            var buffer = new byte[bufferSize];
            var r = reporter == null ? null : new WriteProgressReporter(x => reporter(bytesWritten + x));
            Func<Task> action = null;
            action = () =>
            {
                var count = this.Content.Read(buffer, 0, bufferSize);
                if (count == 0) return InternalUtils.CompletedTask;
                return writer(buffer, 0, count, cancellationToken, r)
                    .Done(() =>
                    {
                        bytesWritten += count;
                        return action();
                    }, cancellationToken)
                    .Unwrap();
            };
            return action();
        }
#endif
    }

    internal class ArraySegmentMultipartItem : FileMultipartItem
    {
        public ArraySegment<byte> Content { get; }

        public ArraySegmentMultipartItem(string name, ArraySegment<byte> content)
            : base(name)
        {
            this.Content = content;
        }

        public override long? ContentLength => this.Content.Count;

#if !ASYNC_ONLY
        public override void WriteTo(Writer writer)
        {
            writer(this.Content.Array, this.Content.Offset, this.Content.Count);
        }
#endif

#if !NET35
        public override Task WriteToAsync(AsyncWriter writer, CancellationToken cancellationToken, WriteProgressReporter reporter)
        {
            return writer(this.Content.Array, this.Content.Offset, this.Content.Count, cancellationToken, reporter);
        }
#endif
    }

    internal class ByteArrayMultipartItem : FileMultipartItem
    {
        public byte[] Content { get; }

        public ByteArrayMultipartItem(string name, byte[] content)
            : base(name)
        {
            this.Content = content;
        }

        public override long? ContentLength => this.Content.Length;

#if !ASYNC_ONLY
        public override void WriteTo(Writer writer)
        {
            writer(this.Content, 0, this.Content.Length);
        }
#endif

#if !NET35
        public override Task WriteToAsync(AsyncWriter writer, CancellationToken cancellationToken, WriteProgressReporter reporter)
        {
            return writer(this.Content, 0, this.Content.Length, cancellationToken, reporter);
        }
#endif
    }

    internal class ByteEnumerableMultipartItem : FileMultipartItem
    {
        public IEnumerable<byte> Content { get; }

        public ByteEnumerableMultipartItem(string name, IEnumerable<byte> content)
            : base(name)
        {
            this.Content = content;
        }

        public override long? ContentLength => (this.Content as ICollection<byte>)?.Count;
        
#if !ASYNC_ONLY
        public override void WriteTo(Writer writer)
        {
            var buffer = new byte[bufferSize];
            var i = 0;
            foreach (var b in this.Content)
            {
                buffer[i++] = b;
                if (i == bufferSize)
                {
                    writer(buffer, 0, bufferSize);
                    i = 0;
                }
            }
            if (i > 0)
            {
                writer(buffer, 0, i);
            }
        }
#endif

#if !NET35
        public override Task WriteToAsync(AsyncWriter writer, CancellationToken cancellationToken, WriteProgressReporter reporter)
        {
            int bytesWritten = 0;
            var enumerator = this.Content.GetEnumerator();
            var buffer = new byte[bufferSize];
            var r = reporter == null ? null : new WriteProgressReporter(x => reporter(bytesWritten + x));
            Func<Task> action = null;
            action = () =>
            {
                var i = 0;
                while (enumerator.MoveNext())
                {
                    buffer[i++] = enumerator.Current;
                    if (i == bufferSize)
                    {
                        return writer(buffer, 0, bufferSize, cancellationToken, r)
                            .Done(() =>
                            {
                                bytesWritten += bufferSize;
                                return action();
                            }, cancellationToken)
                            .Unwrap();
                    }
                }
                return i == 0 ? InternalUtils.CompletedTask
                    : writer(buffer, 0, i, cancellationToken, r);
            };
            return action();
        }
#endif
    }

#if !(PCL || WIN_RT)
    internal class FileInfoMultipartItem : FileMultipartItem
    {
        public FileInfo Content { get; }

        public FileInfoMultipartItem(string name, FileInfo content)
            : base(name)
        {
            this.Content = content;
        }

        public override long? ContentLength => this.Content.Length;
        public override string FileName => this.Content.Name;

#if !ASYNC_ONLY
        public override void WriteTo(Writer writer)
        {
            using (var stream = this.Content.OpenRead())
            {
                var buffer = new byte[bufferSize];
                int count;
                while ((count = stream.Read(buffer, 0, bufferSize)) > 0)
                    writer(buffer, 0, count);
            }
        }
#endif

#if !NET35
        public override Task WriteToAsync(AsyncWriter writer, CancellationToken cancellationToken, WriteProgressReporter reporter)
        {
            var stream = this.Content.OpenRead();
            int bytesWritten = 0;
            var buffer = new byte[bufferSize];
            var r = reporter == null ? null : new WriteProgressReporter(x => reporter(bytesWritten + x));
            Func<Task> action = null;
            action = () =>
            {
                var count = stream.Read(buffer, 0, bufferSize);
                if (count == 0) return InternalUtils.CompletedTask;
                return writer(buffer, 0, count, cancellationToken, r)
                    .Done(() =>
                    {
                        bytesWritten += count;
                        return action();
                    }, cancellationToken)
                    .Unwrap();
            };
            return action().Finally(stream.Close);
        }
#endif
    }
#endif
}
