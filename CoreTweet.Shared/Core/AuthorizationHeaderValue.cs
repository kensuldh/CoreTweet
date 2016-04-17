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

namespace CoreTweet.Core
{
    /// <summary>
    /// Represents authentication information in Authorization header values.
    /// </summary>
    public struct AuthorizationHeaderValue
    {
        /// <summary>
        /// Gets the authentication scheme.
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Gets the additional information necessary for achieving authentication.
        /// </summary>
        public string Parameter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationHeaderValue"/> class.
        /// </summary>
        /// <param name="schema">The authentication scheme.</param>
        /// <param name="parameter">The additional information necessary for achieving authentication.</param>
        public AuthorizationHeaderValue(string schema, string parameter)
        {
            this.Schema = schema;
            this.Parameter = parameter;
        }

        /// <summary>
        /// Returns a string that can be set to Authorization header.
        /// </summary>
        /// <returns>A string that can be set to Authorization header.</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(this.Parameter) ? this.Schema : this.Schema + " " + this.Parameter;
        }
    }
}
