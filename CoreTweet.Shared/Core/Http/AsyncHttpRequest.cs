﻿// The MIT License (MIT)
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

#if !NET35
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreTweet.Core.Http
{
    public class AsyncHttpRequest
    {
        public MethodType Method { get; }
        public Uri RequestUri { get; }
        public ConnectionOptions ConnectionOptions { get; }
        public IList<KeyValuePair<string, string>> CustomHeaders { get; } = new List<KeyValuePair<string, string>>();
        public IAsyncRequestContentWriter Content { get; set; }

        public AsyncHttpRequest(MethodType method, Uri requestUri, ConnectionOptions options)
        {
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));
            this.Method = method;
            this.RequestUri = requestUri;
            this.ConnectionOptions = options ?? new ConnectionOptions();
        }

        //TODO: Task<AsyncHttpResponse> SendAsync リクエスト送信はプラットフォームごとに実装
    }
}
#endif
