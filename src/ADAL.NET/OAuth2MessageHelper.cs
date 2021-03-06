﻿//----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//----------------------------------------------------------------------

using System;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal static partial class OAuth2MessageHelper
    {
        public static RequestParameters CreateTokenRequest(string code, Uri redirectUri, string resource, ClientKey clientKey)
        {
            RequestParameters parameters = new RequestParameters();
            parameters[OAuthParameter.GrantType] = OAuthGrantType.AuthorizationCode;
            parameters[OAuthParameter.Code] = code;
            parameters[OAuthParameter.RedirectUri] = redirectUri.AbsoluteUri;

            AddOptionalParameterResource(parameters, resource);
            AddClientKey(parameters, clientKey);

            return parameters;
        }

        public static RequestParameters CreateTokenRequest(string resource, ClientKey clientKey)
        {
            RequestParameters parameters = new RequestParameters();
            parameters[OAuthParameter.GrantType] = OAuthGrantType.ClientCredentials;
            parameters[OAuthParameter.Resource] = resource;

            AddClientKey(parameters, clientKey);

            return parameters;
        }

        internal static RequestParameters CreateTokenRequest(string resource, string refreshToken, string clientId, ClientKey clientKey)
        {
            RequestParameters parameters = new RequestParameters();
            parameters[OAuthParameter.GrantType] = OAuthGrantType.RefreshToken;
            parameters[OAuthParameter.RefreshToken] = refreshToken;
            parameters[OAuthParameter.ClientId] = clientId;

            AddClientKey(parameters, clientKey);
            AddOptionalParameterResource(parameters, resource);

            return parameters;
        }

        internal static RequestParameters CreateTokenRequest(string resource, UserAssertion userCredential, ClientKey clientKey)
        {
            RequestParameters parameters = new RequestParameters();
            parameters[OAuthParameter.GrantType] = OAuthGrantType.JwtBearer;
            parameters[OAuthParameter.Assertion] = userCredential.Assertion;
            parameters[OAuthParameter.RequestedTokenUse] = OAuthRequestedTokenUse.OnBehalfOf;
            parameters[OAuthParameter.Resource] = resource;

            // To request id_token in response
            parameters[OAuthParameter.Scope] = ScopeOpenIdValue;

            AddClientKey(parameters, clientKey);

            return parameters;
        }
        
        private static void AddClientKey(RequestParameters parameters, ClientKey clientKey)
        {
            if (clientKey.Credential != null)
            {
                if (!parameters.ContainsKey(OAuthParameter.ClientId))
                {
                    parameters[OAuthParameter.ClientId] = clientKey.Credential.ClientId;
                }

                if (clientKey.Credential.ClientSecret != null)
                {
                    parameters[OAuthParameter.ClientSecret] = clientKey.Credential.ClientSecret;
                }
                else
                {
                    parameters.AddSecureParameter(OAuthParameter.ClientSecret, clientKey.Credential.SecureClientSecret);
                }
            }
            else if (clientKey.Assertion != null)
            {
                parameters[OAuthParameter.ClientAssertionType] = clientKey.Assertion.AssertionType;
                parameters[OAuthParameter.ClientAssertion] = clientKey.Assertion.Assertion;
            }
            else if (clientKey.Certificate != null)
            {
                JsonWebToken jwtToken = new JsonWebToken(clientKey.Audience, clientKey.Certificate.ClientId, AuthenticationConstant.JwtToAadLifetimeInSeconds, clientKey.Certificate.ClientId);
                ClientAssertion clientAssertion = jwtToken.Sign(clientKey.Certificate);
                parameters[OAuthParameter.ClientAssertionType] = clientAssertion.AssertionType;
                parameters[OAuthParameter.ClientAssertion] = clientAssertion.Assertion;
            }
            else
            {
                throw new ArgumentException("clientKey");
            }
        }
    }
}
