﻿// The MIT License (MIT)
//
// CoreTweet - A .NET Twitter Library supporting Twitter API 1.1
// Copyright (c) 2014 lambdalice
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

using CoreTweet.Core;
using Newtonsoft.Json;

namespace CoreTweet
{
    /// <summary>
    /// Represents the error response from Twitter.
    /// </summary>
    public class Error : CoreBase
    {
        /// <summary>
        /// <para>Gets or sets the machine-parsable code.</para>
        /// <para>While the text for an error message may change, the codes will stay the same.</para>
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Represents machine-parseable error code from Twitter (convertible from/to <see cref="Error.Code"/>).
    /// </summary>
    public enum ErrorCode : int
    {
        /// <summary>
        /// The request could not be completed as requested.
        /// </summary>
        CouldNotAuthenticate = 32,
        /// <summary>
        /// Corresponds with a HTTP 404 - The specified resource was not found.
        /// </summary>
        PageDoesNotExist = 34,
        /// <summary>
        /// Corresponds with a HTTP 403 - The user is suspended (as well as its access token) and could not complete the request.
        /// </summary>
        AccountIsSuspended = 64,
        /// <summary>
        /// The request was a REST API v1 request (which is no longer available).
        /// </summary>
        RestApiV1IsNoLongerActive = 68,
        /// <summary>
        /// The request could not be completed because it reached the current rate limit window.
        /// </summary>
        RateLimitExceeded = 88,
        /// <summary>
        /// The access token used in the request is incorrect or has been expired.
        /// </summary>
        InvalidOrExpiredToken = 89,
        /// <summary>
        /// Only SSL connections are allowed in specified request.
        /// </summary>
        SslIsRequired = 92,
        /// <summary>
        /// Undocumented: Credentials in the request is invalid (thrown on application-only authentication).
        /// </summary>
        UnableToVerifyCredentials = 99,
        /// <summary>
        /// Corresponds with a HTTP 503 - Twitter is temporarily over capacity.
        /// </summary>
        OverCapacity = 130,
        /// <summary>
        /// Corresponds with a HTTP 500 - An unknown internal occurred while Twitter trying to serve the request.
        /// </summary>
        InternalError = 131,
        /// <summary>
        /// Corresponds with a HTTP 401 - The oauth_timestamp of the request is either ahead or behind its acceptable range.
        /// </summary>
        CouldNotAuthenticate_TimestampIsInvalid = 135,
        /// <summary>
        /// Undocumented: The authenticating user have already favorited specified status.
        /// </summary>
        AlreadyFavorited = 139,
        /// <summary>
        /// Undocumented: The authenticating user cannot send a direct message to a user not following the authenticating user.
        /// </summary>
        CannotSendDirectMessagesToUsersNotFollowingMe = 150,
        /// <summary>
        /// Undocumented: The direct message is too long to send.
        /// </summary>
        CannotSendLongDirectMessages = 151,
        /// <summary>
        /// Undocumented: The authenticating user have already requested to follow specified user.
        /// </summary>
        AlreadyFollowRequested = 160,
        /// <summary>
        /// Corresponds with a HTTP 403 - The follow request could not be completed due to some kind of limit.
        /// </summary>
        UnableToFollowMorePeople = 161,
        /// <summary>
        /// Corresponds with a HTTP 403 - The requested Tweet cannot be viewed by the authenticating user.
        /// </summary>
        NotAuthorizedToSeeStatus = 179,
        /// <summary>
        /// Undocumented: Can't delete other users' status.
        /// </summary>
        CannotDeleteOtherUsersStatus = 183,
        /// <summary>
        /// Corresponds with a HTTP 403 - The authenticating user reached some kind of status update limit.
        /// </summary>
        OverStatusUpdateLimit = 185,
        /// <summary>
        /// The status text have been Tweeted already by the authenticated user.
        /// </summary>
        StatusIsDuplicate = 187,
        /// <summary>
        /// Authentication data of the request was invalid or missing.
        /// </summary>
        BadAuthenticationData = 215,
        /// <summary>
        /// Twitter detected automated actions and could not complete the request to prevent malicious activities.
        /// </summary>
        DetectedAutomatedAction = 226,
        /// <summary>
        /// Authenticating user must verify login using Twitter's "login verification" feature.
        /// </summary>
        MustVerifyLogin = 231,
        /// <summary>
        /// The request to a retired URL cannot be completed.
        /// </summary>
        EndpointHasRetired = 251,
        /// <summary>
        /// Corresponds with a HTTP 403 - The application is restricted from performing write-related actions.
        /// </summary>
        CannotPerformWriteActions = 261,
        /// <summary>
        /// Corresponds with a HTTP 403 - The authenticated used cannot mute itself.
        /// </summary>
        CannotMuteMyself = 271,
        /// <summary>
        /// Corresponds with a HTTP 403 - The unmute request could not be completed because the authenticating user is not muting the specified user.
        /// </summary>
        NotMutingSpecifiedUser = 272,
    }
}
