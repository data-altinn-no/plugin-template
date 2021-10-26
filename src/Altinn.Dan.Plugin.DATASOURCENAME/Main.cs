using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Altinn.Dan.Plugin.DATASOURCENAME.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
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
        private HttpClient _client;
        private ApplicationSettings _settings;
        private EvidenceSourceMetadata _metadata;

        public Main(IHttpClientFactory httpClientFactory, IApplicationSettings settings)
        {
            _client = httpClientFactory.CreateClient("SafeHttpClient");
            _settings = (ApplicationSettings)settings;
            _metadata = new EvidenceSourceMetadata(_settings);
        }

        [FunctionName("DATASETNAME1")]
        public async Task<IActionResult> Bemanning(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            _logger = log;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesDatasetName1(evidenceHarvesterRequest));
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesDatasetName1(EvidenceHarvesterRequest evidenceHarvesterRequest)
        {
            dynamic content = await MakeRequest(string.Format(_settings.DATASETNAME1URL, evidenceHarvesterRequest.OrganizationNumber), evidenceHarvesterRequest.OrganizationNumber);

            var ecb = new EvidenceBuilder(_metadata, "DATASETNAME1");
            ecb.AddEvidenceValue($"felt1", content.Organisasjonsnummer, EvidenceSourceMetadata.SOURCE);
            ecb.AddEvidenceValue($"felt2", content.Godkjenningsstatus, EvidenceSourceMetadata.SOURCE);

            return ecb.GetEvidenceValues();
        }

        private async Task<dynamic> MakeRequest(string target, string organizationNumber)
        {
            HttpResponseMessage result = null;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, target);
                result = await _client.SendAsync(request);
            }
            catch (HttpRequestException ex)
            {
                throw new EvidenceSourcePermanentServerException(EvidenceSourceMetadata.ERROR_CCR_UPSTREAM_ERROR, null, ex);
            }

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                throw new EvidenceSourcePermanentClientException(EvidenceSourceMetadata.ERROR_ORGANIZATION_NOT_FOUND, $"{organizationNumber} could not be found");
            }

            var response = JsonConvert.DeserializeObject(await result.Content.ReadAsStringAsync());

            if (response == null)
            {
                throw new EvidenceSourcePermanentServerException(EvidenceSourceMetadata.ERROR_CCR_UPSTREAM_ERROR,
                    "Did not understand the data model returned from upstream source");
            }

            return response;
        }

        [FunctionName("DATSETNAME2")]
        public async Task<IActionResult> Renhold(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            _logger = log;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

            return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesDatasetName2(evidenceHarvesterRequest));
        }

        private async Task<List<EvidenceValue>> GetEvidenceValuesDatasetName2(EvidenceHarvesterRequest evidenceHarvesterRequest)
        {
            dynamic content = await MakeRequest(string.Format(_settings.DATASETNAME2URL, evidenceHarvesterRequest.OrganizationNumber), evidenceHarvesterRequest.OrganizationNumber);

            var ecb = new EvidenceBuilder(_metadata, "DATASETNAME2");
            ecb.AddEvidenceValue($"felt1", content.responsfelt1, EvidenceSourceMetadata.SOURCE);
            ecb.AddEvidenceValue($"felt2", content.responsfelt2, EvidenceSourceMetadata.SOURCE);
            ecb.AddEvidenceValue($"felt3", content.responsfelt3, EvidenceSourceMetadata.SOURCE);

            return ecb.GetEvidenceValues();
        }

        [FunctionName(Constants.EvidenceSourceMetadataFunctionName)]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            var response = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(_metadata.GetEvidenceCodes(), typeof(List<EvidenceCode>), new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore
                })),
                RequestMessage = req
            };

            return response;
        }
    }
}
