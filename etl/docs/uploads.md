## Uploading Participant Data

> ⚠️ This documentation describes the bulk upload API and a _temporary_ upload approach using AzCopy that should allow states to quickly provide test data to our system with out expending development effort unnecessarily.

Once you have [validated the format](./bulk-import.md) of your participant data CSV, you can upload the file to the system through the bulk upload API or AzCopy.

### AzCopy

AzCopy is a Microsoft-supported command line utility that you can use to upload files to Azure blob storage resources.

[Download AzCopy for your environment](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-v10#download-azcopy)

#### Authorization

Contact a project representative to gain the necessary credentials for uploading through AzCopy. What you will need:

- application id
- tenant id
- password (this is used as the client secret)
- storage account url

All of these items will vary between test and production environments.

Upon receiving these credentials, you'll first authorize AzCopy through a [service principal approach](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-authorize-azure-active-directory?toc=/azure/storage/blobs/toc.json#authorize-a-service-principal).

First, save the password to the `AZCOPY_SPA_CLIENT_SECRET` environment variable, taking care to avoid saving it in your command-line history:

PowerShell: `$env:AZCOPY_SPA_CLIENT_SECRET="$(Read-Host -prompt "Enter key")"`

Bash: `read -s -p "Enter key " AZCOPY_SPA_CLIENT_SECRET && export AZCOPY_SPA_CLIENT_SECRET`

Then login using the application id and tenant id:

```
azcopy login --service-principal  --application-id application-id --tenant-id=tenant-id
```

Login command will return a success or failure message. For more more details on authorizing AzCopy, visit the [Microsoft Documentation](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-authorize-azure-active-directory?toc=/azure/storage/blobs/toc.json#authorize-a-service-principal-by-using-a-client-secret).

#### Uploading a File

After authorization succeeds, you can upload your file using the storage account url:

Example:
```
$ azcopy copy 'name-of-file.csv' 'https://my-storage-account-name.blob.core.windows.net/upload'
```

For more details on uploading files through AzCopy, visit the [Microsoft documentation](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-blobs-upload).

### Bulk upload API

The bulk upload API consists of a single HTTP `PUT` endpoint that you can use to upload the participant data CSV file.

#### Authorization

Contact a project representative to gain the necessary information for calling the API. You will receive:
- An API endpoint specific to your state, in the format `https://<api-domain>/<state-identifier>/upload/`
- An API key

#### Uploading a file

To upload a file, make an API call that conforms to the [API spec](openapi/openapi.yaml).

First, construct the URL for the API call by appending the filename to endpoint you were given. For example:
```
https://<api-domain>/<state-identifer>/upload/bulk-data.csv
```

Second, send a `PUT` request to the endpoint and include the following headers:
- `Ocp-Apim-Subscription-Key: <api-key>` — where `<api-key>` is the API key you were provided.
- `Content-Length: <filesize>` — where `<filesize>` is the size of the file you are uploading, in bytes. Many tools (like `curl`, Powershell's `Invoke-WebRequest`, or [Postman](https://www.postman.com/)) will automatically include this header for you.

If your file is successfully uploaded you will receive an HTTP response with a `201 created` status.

##### Example using Powershell

To call the API from Powershell using `Invoke-WebRequest`, run the following command substituting `<endpoint-url>` with the full endpoint URL (containing filename), `<api-key>` with your API key, and `<path-to-csv>` with the path the CSV file you are uploading.

```
$url = '<endpoint-url>'
$headers = @{
    'Ocp-Apim-Subscription-Key' = '<api-key>'
}
$body = Get-Content <path-to-csv> -Raw
Invoke-WebRequest -Uri $url -Method Put -Headers $headers -Body $body
```

*Note: `Invoke-WebRequest` automatically includes the required `Content-Length` header.*

##### Example using `curl`

To call the API from a bash shell using `curl`, run the following command substituting `<endpoint-url>` with the full endpoint URL (containing filename), `<api-key>` with your API key, and `<path-to-csv>` with the path the CSV file you are uploading.

```
curl --location --request PUT '<endpoint-url>' \
--header 'Ocp-Apim-Subscription-Key: <api-key>' \
--upload-file "<path-to-csv>"  \
--include
```

*Note: `curl` automatically includes the required `Content-Length` header.*
