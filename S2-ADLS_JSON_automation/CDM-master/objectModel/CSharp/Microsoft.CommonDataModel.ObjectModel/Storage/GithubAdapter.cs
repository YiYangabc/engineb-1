﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.CommonDataModel.ObjectModel.Storage
{
    using Microsoft.CommonDataModel.ObjectModel.Utilities;
    using Microsoft.CommonDataModel.ObjectModel.Utilities.Network;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class GithubAdapter : NetworkAdapter, StorageAdapter
    {
        private static string ghHost = "raw.githubusercontent.com";
        private static string ghPath = "/Microsoft/CDM/master/schemaDocuments";

        /// <inheritdoc />
        public string LocationHint { get; set; }

        internal const string Type = "github";

        /// <summary>
        /// Constructs a GithubAdapter.
        /// </summary>
        public GithubAdapter()
        {
            this.httpClient = new CdmHttpClient($"https://{ghHost}");
        }

        private static string GhRawRoot()
        {
            return $"https://{ghHost}{ghPath}";
        }

        /// <inheritdoc />
        public bool CanRead()
        {
            return true;
        }

        /// <inheritdoc />
        public async Task<string> ReadAsync(string corpusPath)
        {
            var httpRequest = this.SetUpCdmRequest($"{ghPath}{corpusPath}", 
                new Dictionary<string, string>() { { "User-Agent", "CDM" } }, HttpMethod.Get);

            var cdmResponse = await base.ExecuteRequest(httpRequest);

            return await cdmResponse.Content.ReadAsStringAsync();
        }

        /// <inheritdoc />
        public bool CanWrite()
        {
            return false;
        }

        /// <inheritdoc />
        public Task WriteAsync(string corpusPath, string data)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void ClearCache()
        {
            
        }

        /// <inheritdoc />
        public Task<DateTimeOffset?> ComputeLastModifiedTimeAsync(string corpusPath)
        {
            return Task.FromResult<DateTimeOffset?>(DateTimeOffset.UtcNow);
        }

        /// <inheritdoc />
        public async Task<List<string>> FetchAllFilesAsync(string currFullPath)
        {
            // TODO
            return null;
        }

        /// <inheritdoc />
        public string CreateAdapterPath(string corpusPath)
        {
            return $"{GithubAdapter.GhRawRoot()}{corpusPath}";
        }

        /// <inheritdoc />
        public string CreateCorpusPath(string adapterPath)
        {
            string ghRoot = GithubAdapter.GhRawRoot();
            // might not be an adapterPath that we understand. check that first 
            if (!string.IsNullOrEmpty(adapterPath) && adapterPath.StartsWith(ghRoot))
            {
                return StringUtils.Slice(adapterPath, ghRoot.Length);
            }

            return null;
        }

        /// <inheritdoc />
        public string FetchConfig()
        {
            var resultConfig = new JObject
            {
                { "type", Type }
            };

            var configObject = new JObject
            {
                // Construct network configs.
                this.FetchNetworkConfig()
            };

            if (this.LocationHint != null)
            {
                configObject.Add("locationHint", this.LocationHint);
            }

            resultConfig.Add("config", configObject);

            return resultConfig.ToString();
        }

        /// <inheritdoc />
        public void UpdateConfig(string config)
        {
            if (config == null)
            {
                // It is fine just to skip it for GitHub adapter.
                return;
            }

            this.UpdateNetworkConfig(config);

            var configJson = JsonConvert.DeserializeObject<JObject>(config);

            if (configJson["locationHint"] != null)
            {
                this.LocationHint = configJson["locationHint"].ToString();
            }
        }
    }
}
