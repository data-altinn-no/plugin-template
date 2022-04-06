using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Altinn.Dan.Plugin.DATASOURCENAME.Config;
using Altinn.Dan.Plugin.DATASOURCENAME.Models;
using Azure.Core.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nadobe;
using Nadobe.Common.Exceptions;
using Nadobe.Common.Models;
using Nadobe.Common.Util;
using Newtonsoft.Json;

namespace Altinn.Dan.Plugin.DATASOURCENAME
{
    public class Main
    {
        private ILogger _logger;
        private readonly HttpClient _client;
        private readonly ApplicationSettings _settings;

        public Main(IHttpClientFactory httpClientFactory, IOptions<ApplicationSettings> settings)
        {
            _client = httpClientFactory.CreateClient("SafeHttpClient");
            _settings = settings.Value;
        }

        [Function("DATASETNAME1")]
        public async Task<HttpResponseData> Dataset1(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            _logger.LogInformation("Running func 'DATASETNAME1'");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            var actionResult = await EvidenceSourceResponse.CreateResponse(null, () => GetEvidenceValuesDatasetName1(evidenceHarvesterRequest)) as ObjectResult;
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(actionResult?.Value);

            return response;
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesDatasetName1(EvidenceHarvesterRequest evidenceHarvesterRequest)
        {
            var content = await PostRequest<DatasourceResponse>(_settings.DATASETNAME1URL, evidenceHarvesterRequest.SubjectParty);

            var ecb = new EvidenceBuilder(new Metadata(), "DATASETNAME1");
            ecb.AddEvidenceValue($"field1", content.ResponseField1, Metadata.SOURCE);
            ecb.AddEvidenceValue($"field2", content.ResponseField2, Metadata.SOURCE);

            return ecb.GetEvidenceValues();
        }

        private async Task<T> PostRequest<T>(string target, Party subject) where T : new()
        {
            HttpResponseMessage response = null;
            try
            {
                var datasourceRequest = new DatasourceRequest
                {
                    RequestField1 = subject.NorwegianSocialSecurityNumber,
                    RequestField2 = subject.Id
                };

                var body = new StringContent(JsonConvert.SerializeObject(datasourceRequest));
                body.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                response = await _client.PostAsync(target, body);
                var responseContent = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                    {
                        return JsonConvert.DeserializeObject<T>(responseContent);
                    }
                    default:
                    {
                        throw new EvidenceSourcePermanentClientException(Metadata.ERROR_CCR_UPSTREAM_ERROR,
                            $"External API call failed ({(int)response.StatusCode} - {response.StatusCode})" + (string.IsNullOrEmpty(responseContent) ? string.Empty : $", details: {responseContent}"));
                    }
                }
            }
            catch (HttpRequestException e)
            {
                throw new EvidenceSourcePermanentServerException(Metadata.ERROR_CCR_UPSTREAM_ERROR, null, e);
            }
            finally
            {
                response?.Dispose();
            }
        }

        [Function("DATASETNAME2")]
        public async Task<HttpResponseData> Dataset2(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            _logger.LogInformation("Running func 'DATASETNAME2'");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            var actionResult = await EvidenceSourceResponse.CreateResponse(null, () => GetEvidenceValuesDatasetName2(evidenceHarvesterRequest)) as ObjectResult;
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(actionResult?.Value);

            return response;
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesDatasetName2(EvidenceHarvesterRequest evidenceHarvesterRequest)
        {
            var content = await GetRequest<DatasourceResponse>(_settings.DATASETNAME2URL, evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber);

            var ecb = new EvidenceBuilder(new Metadata(), "DATASETNAME2");
            ecb.AddEvidenceValue($"default", JsonConvert.SerializeObject(content), Metadata.SOURCE);

            return ecb.GetEvidenceValues();
        }

        private async Task<T> GetRequest<T>(string target, string organizationNumber) where T : new()
        {
            HttpResponseMessage response = null;
            try
            {
                var completeUrl = target.Replace("{orgNo}", Uri.EscapeDataString(organizationNumber));
                response = await _client.GetAsync(completeUrl);
                var responseData = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                    {
                        return JsonConvert.DeserializeObject<T>(responseData);
                    }
                    case HttpStatusCode.NotFound:
                    {
                        throw new EvidenceSourcePermanentClientException(Metadata.ERROR_ORGANIZATION_NOT_FOUND,
                            $"{organizationNumber} could not be found");
                    }
                    default:
                    {
                        throw new EvidenceSourcePermanentClientException(Metadata.ERROR_CCR_UPSTREAM_ERROR,
                            $"External API call to Kartverket failed ({(int)response.StatusCode} - {response.StatusCode})" + (string.IsNullOrEmpty(responseData) ? string.Empty : $", details: {responseData}"));
                    }
                }
            }
            catch (HttpRequestException e)
            {
                throw new EvidenceSourcePermanentServerException(Metadata.ERROR_CCR_UPSTREAM_ERROR, null, e);
            }
            finally
            {
                response?.Dispose();
            }
        }

        [Function(Constants.EvidenceSourceMetadataFunctionName)]
        public async Task<HttpResponseData> GetMetadata(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            _logger = context.GetLogger(context.FunctionDefinition.Name);
            _logger.LogInformation($"Running func metadata for {Constants.EvidenceSourceMetadataFunctionName}");
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new Metadata().GetEvidenceCodes(),
                new NewtonsoftJsonObjectSerializer(new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto }));

            return response;
        }
    }
}
