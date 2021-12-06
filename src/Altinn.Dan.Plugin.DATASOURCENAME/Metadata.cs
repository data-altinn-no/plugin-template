using System.Collections.Generic;
using Altinn.Dan.Plugin.DATASOURCENAME.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nadobe.Common.Interfaces;
using Nadobe.Common.Models;
using Nadobe.Common.Models.Enums;

namespace Altinn.Dan.Plugin.DATASOURCENAME
{
    public class Metadata
    {
        private readonly ILogger<Metadata> _log;
        private ApplicationSettings _settings;

        private const string SERIVCECONTEXT_EBEVIS = "servicecontext ie ebevis";

        public Metadata(ILogger<Metadata> log, IOptions<ApplicationSettings> settings)
        {
            _log = log;
            _settings = settings.Value;
        }

        public List<EvidenceCode> GetEvidenceCodes()
        {
            _log.LogInformation($"Fetching EvidenceCodes");
            var a = new List<EvidenceCode>()
            {
                new EvidenceCode()
                {
                    EvidenceCodeName = "DATASETNAME1",
                    EvidenceSource = EvidenceSourceMetadata.SOURCE,
                    BelongsToServiceContexts = new List<string>() { SERIVCECONTEXT_EBEVIS },
                    AccessMethod = EvidenceAccessMethod.Open,
                    Values = new List<EvidenceValue>()
                    {
                        new EvidenceValue()
                        {
                            EvidenceValueName = "field1",
                            ValueType = EvidenceValueType.String
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "field2",
                            ValueType = EvidenceValueType.String
                        }
                    }
                },
                new EvidenceCode()
                {
                    EvidenceCodeName = "DATASETNAME2",
                    EvidenceSource = EvidenceSourceMetadata.SOURCE,
                    BelongsToServiceContexts = new List<string>() { SERIVCECONTEXT_EBEVIS },
                    AccessMethod = EvidenceAccessMethod.Open,
                    Values = new List<EvidenceValue>()
                    {
                        new EvidenceValue()
                        {
                            EvidenceValueName = "field1",
                            ValueType = EvidenceValueType.String
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "field2",
                            ValueType = EvidenceValueType.String
                        },
                        new EvidenceValue()
                        {
                            EvidenceValueName = "field3",
                            ValueType = EvidenceValueType.DateTime
                        }
                    }
                }
            };

            return a;
        }
    }

    public class EvidenceSourceMetadata : IEvidenceSourceMetadata
    {
        public const string SOURCE = "DATASOURCENAME";

        public const int ERROR_ORGANIZATION_NOT_FOUND = 1;

        public const int ERROR_CCR_UPSTREAM_ERROR = 2;

        public const int ERROR_NO_REPORT_AVAILABLE = 3;

        public const int ERROR_ASYNC_REQUIRED_PARAMS_MISSING = 4;

        public const int ERROR_ASYNC_ALREADY_INITIALIZED = 5;

        public const int ERROR_ASYNC_NOT_INITIALIZED = 6;

        public const int ERROR_AYNC_STATE_STORAGE = 7;

        public const int ERROR_ASYNC_HARVEST_NOT_AVAILABLE = 8;

        public const int ERROR_CERTIFICATE_OF_REGISTRATION_NOT_AVAILABLE = 9;

        private Metadata _metadata;
        private ApplicationSettings _settings;

        public EvidenceSourceMetadata(Metadata metadata, IOptions<ApplicationSettings> settings)
        {
            _metadata = metadata;
            _settings = settings.Value;
        }

        public List<EvidenceCode> GetEvidenceCodes()
        {
            return _metadata.GetEvidenceCodes();
        }
    }
}
