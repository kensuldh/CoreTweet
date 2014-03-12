// The MIT License (MIT)
//
// CoreTweet - A .NET Twitter Library supporting Twitter API 1.1
// Copyright (c) 2013 lambdalice
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
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using CoreTweet.Core;
using Alice.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;


namespace CoreTweet
{
    public static class OAuth
    {
        public class OAuthSession
        {
            public string ConsumerKey { get; set; }
            public string ConsumerSecret { get; set; }
            public string RequestToken { get; set; }
            public string RequestTokenSecret { get; set; }
            public Uri AuthorizeUri
            {
                get
                {
                    return new Uri(AuthorizeUrl + "?oauth_token=" + RequestToken);
                }
            }
        }

        /// <summary>
        /// The request token URL.
        /// </summary>
        static readonly string RequestTokenUrl = "https://api.twitter.com/oauth/request_token";
        /// <summary>
        /// The access token URL.
        /// </summary>
        static readonly string AccessTokenUrl = "https://api.twitter.com/oauth/access_token";
        /// <summary>
        /// The authorize URL.
        /// </summary>
        static readonly string AuthorizeUrl = "https://api.twitter.com/oauth/authorize";

        /// <summary>
        ///     Generates the authorize URI.
        ///     Then call GetTokens(string) after get the pin code.
        /// </summary>
        /// <returns>
        ///     The authorize URI.
        /// </returns>
        /// <param name='consumer_key'>
        ///     Consumer key.
        /// </param>
        /// <param name='consumer_secret'>
        ///     Consumer secret.
        /// </param>
        public static OAuthSession Authorize(string consumerKey, string consumerSecret)
        {
            var header = Tokens.Create(consumerKey, consumerSecret, null, null)
                .CreateAuthorizationHeader(MethodType.Get, RequestTokenUrl, null);
            var dic = from x in Request.HttpGet(RequestTokenUrl, null, header).Use()
                      from y in new StreamReader(x).Use()
                      select y.ReadToEnd()
                              .Split('&')
                              .Where(z => z.Contains('='))
                              .Select(z => z.Split('='))
                              .ToDictionary(z => z[0], z => z[1]);
            return new OAuthSession()
            {
                RequestToken = dic["oauth_token"],
                RequestTokenSecret = dic["oauth_token_secret"],
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret
            };
        }

        /// <summary>
        ///     Gets the OAuth tokens.
        ///     Be sure to call GenerateAuthUri(string,string) before call this.
        /// </summary>
        /// <param name='pin'>
        ///     Pin code.
        /// </param>
        /// <param name~'session'>
        ///     OAuth session.
        /// </para>
        /// <returns>
        ///     The tokens.
        /// </returns>
        public static Tokens GetTokens(this OAuthSession session, string pin)
        {
            var prm = new Dictionary<string, object>() { { "oauth_verifier", pin } };
            var header = Tokens.Create(session.ConsumerKey, session.ConsumerSecret, session.RequestToken, session.RequestTokenSecret)
                .CreateAuthorizationHeader(MethodType.Get, AccessTokenUrl, prm);
            var dic = from x in Request.HttpGet(AccessTokenUrl, prm, header).Use()
                      from y in new StreamReader(x).Use()
                      select y.ReadToEnd()
                              .Split('&')
                              .Where(z => z.Contains('='))
                              .Select(z => z.Split('='))
                              .ToDictionary(z => z[0], z => z[1]);
            return Tokens.Create(session.ConsumerKey, session.ConsumerSecret,
                dic["oauth_token"], dic["oauth_token_secret"], long.Parse(dic["user_id"]), dic["screen_name"]);
        }
    }

    public static class OAuth2
    {
        /// <summary>
        /// The access token URL.
        /// </summary>
        static readonly string AccessTokenUrl = "https://api.twitter.com/oauth2/token";
        /// <summary>
        /// The URL to revoke a OAuth2 Bearer Token.
        /// </summary>
        static readonly string InvalidateTokenUrl = "https://api.twitter.com/oauth2/invalidate_token";

        private static string CreateCredentials(string consumerKey, string consumerSecret)
        {
            return "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(consumerKey + ":" + consumerSecret));
        }

        /// <summary>
        /// Gets the OAuth 2 Bearer Token.
        /// </summary>
        /// <param name="consumerKey">Consumer key.</param>
        /// <param name="consumerSecret">Consumer secret.</param>
        /// <returns>The tokens.</returns>
        public static OAuth2Tokens GetToken(string consumerKey, string consumerSecret)
        {
            var token = from x in Request.HttpPost(
                            AccessTokenUrl,
                            new Dictionary<string, object>() { { "grant_type", "client_credentials" } }, //  At this time, only client_credentials is allowed.
                            CreateCredentials(consumerKey, consumerSecret),
                            true).Use()
                        from y in new StreamReader(x).Use()
                        select (string)JObject.Parse(y.ReadToEnd())["access_token"];
            return OAuth2Tokens.Create(consumerKey, consumerSecret, token);
        }

        /// <summary>
        /// Invalidates the OAuth 2 Bearer Token.
        /// </summary>
        /// <param name="tokens">An instance of OAuth2Tokens.</param>
        /// <returns>The invalidated token.</returns>
        public static string InvalidateToken(this OAuth2Tokens tokens)
        {
            return from x in Request.HttpPost(
                       InvalidateTokenUrl,
                       new Dictionary<string, object>() { { "access_token", Uri.UnescapeDataString(tokens.BearerToken) } },
                       CreateCredentials(tokens.ConsumerKey, tokens.ConsumerSecret),
                       true).Use()
                   from y in new StreamReader(x).Use()
                   select (string)JObject.Parse(y.ReadToEnd())["access_token"];
        }
    }
}

