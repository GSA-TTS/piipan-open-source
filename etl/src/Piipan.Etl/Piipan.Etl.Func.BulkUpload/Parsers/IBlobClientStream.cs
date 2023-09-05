using System;
using System.Collections.Generic;
using System.IO;
using Piipan.Participants.Api.Models;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Piipan.Etl.Func.BulkUpload.Parsers
{
    public interface IBlobClientStream
    {
        BlockBlobClient Parse(string input, ILogger log);

        Azure.Response<bool> DeleteBlobAfterProcessing(Task antecedent, BlockBlobClient blockBlobClient, ILogger log);
    }
}
