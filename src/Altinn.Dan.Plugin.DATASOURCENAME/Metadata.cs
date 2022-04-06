using System.Collections.Generic;
using Altinn.Dan.Plugin.DATASOURCENAME.Models;
using Nadobe.Common.Interfaces;
using Nadobe.Common.Models;
using Nadobe.Common.Models.Enums;
using Newtonsoft.Json;
using NJsonSchema;

namespace Altinn.Dan.Plugin.DATASOURCENAME
{
    public class Metadata : IEvidenceSourceMetadata
    {
        private const string SERIVCECONTEXT_EBEVIS = "servicecontext ie ebevis";

        public const string SOURCE = "DATASOURCENAME";
        public const int ERROR_CCR_UPSTREAM_ERROR = 2;
        public const int ERROR_ORGANIZATION_NOT_FOUND = 1;
        public const int ERROR_NO_REPORT_AVAILABLE = 3;
        public const int ERROR_ASYNC_REQUIRED_PARAMS_MISSING = 4;
        public const int ERROR_ASYNC_ALREADY_INITIALIZED = 5;
        public const int ERROR_ASYNC_NOT_INITIALIZED = 6;
        public const int ERROR_AYNC_STATE_STORAGE = 7;
        public const int ERROR_ASYNC_HARVEST_NOT_AVAILABLE = 8;
        public const int ERROR_CERTIFICATE_OF_REGISTRATION_NOT_AVAILABLE = 9;

        public List<EvidenceCode> GetEvidenceCodes()
        {
            return new List<EvidenceCode>()
            {
                new()
                {
                    EvidenceCodeName = "DATASETNAME1",
                    EvidenceSource = SOURCE,
                    BelongsToServiceContexts = new List<string>() { SERIVCECONTEXT_EBEVIS },
                    AccessMethod = EvidenceAccessMethod.Open,
                    Values = new List<EvidenceValue>()
                    {
                        new()
                        {
                            EvidenceValueName = "field1",
                            ValueType = EvidenceValueType.String
                        },
                        new()
                        {
                            EvidenceValueName = "field2",
                            ValueType = EvidenceValueType.String
                        }
                    }
                },
                new()
                {
                    EvidenceCodeName = "DATASETNAME2",
                    EvidenceSource = SOURCE,
                    BelongsToServiceContexts = new List<string>() { SERIVCECONTEXT_EBEVIS },
                    AccessMethod = EvidenceAccessMethod.Open,
                    Values = new List<EvidenceValue>()
                    {
                        new()
                        {
                            EvidenceValueName = "default",
                            ValueType = EvidenceValueType.JsonSchema,
                            JsonSchemaDefintion = JsonSchema.FromType<DatasourceResponse>().ToJson(Formatting.Indented)
                        }
                    },
                    AuthorizationRequirements = new List<Requirement>
                    {
                        new MaskinportenScopeRequirement
                        {
                            RequiredScopes = new List<string> { "altinn:dataaltinnno/scope" }
                        }
                    }
                }
            };
        }
    }
}
