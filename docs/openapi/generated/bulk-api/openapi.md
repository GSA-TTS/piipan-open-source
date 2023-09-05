
<h1 id="bulk-api">Bulk API v2.0.0</h1>

The API for performing bulk uploads.
File Preparation.
This API requires a file to be submitted with the appropriate schema. The details of that file's schema can be found at <a href='../../../../etl/docs/csv/import-schema.json'>File schema</a>

Base URLs:

* <a href="/bulk/{stateAbbr}/v2">/bulk/{stateAbbr}/v2</a>

    * **stateAbbr** - Lowercase two-letter postal code abbreviation Default: none

# Authentication

* API Key (ApiKeyAuth)
    - Parameter Name: **Ocp-Apim-Subscription-Key**, in: header. 

<h1 id="bulk-api-bulk-upload">Bulk Upload</h1>

## upload_all_participants

<a id="opIdupload_all_participants"></a>

> Code samples

```shell
# You can also use wget
curl -X PUT /bulk/{stateAbbr}/v2/upload_all_participants/{filename} \
  -H 'Content-Type: text/plain' \
  -H 'Accept: application/json' \
  -H 'Content-Length: 6413' \
  -H 'Ocp-Apim-Subscription-Key: API_KEY'

```

`PUT /upload_all_participants/{filename}`

*Upload a CSV file of bulk participant data*

Upload a CSV file of bulk participant data. Each tenant is allowed a maximum of 60 requests per minute.

> Body parameter

```
string

```

<h3 id="upload_all_participants-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|filename|path|string|true|Name of file being uploaded|
|Content-Length|header|integer|true|Size in bytes of your file to be uploaded. A curl request will add this header by default when including a data or file parameter.|

<h3 id="upload_all_participants-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|201|[Created](https://tools.ietf.org/html/rfc7231#section-6.3.2)|File uploaded|Inline|
|401|[Unauthorized](https://tools.ietf.org/html/rfc7235#section-3.1)|Access denied|None|
|411|[Length Required](https://tools.ietf.org/html/rfc7231#section-6.5.10)|Content-Length not provided|None|
|413|[Payload Too Large](https://tools.ietf.org/html/rfc7231#section-6.5.11)|Payload Too Large|None|
|415|[Unsupported Media Type](https://tools.ietf.org/html/rfc7231#section-6.5.13)|Unknown file type|None|

<h3 id="upload_all_participants-responseschema">Response Schema</h3>

Status Code **201**

*Upload response*

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» data|object|false|none|The response payload. Will contain a data property with upload details.|
|»» upload_id|string|true|none|A unique upload_id for the successful upload.|

### Response Examples

> An upload of a single CSV file of all the participants

```json
{
  "data": {
    "upload_id": "0x8DA2EA86C4C2089"
  }
}
```

<aside class="warning">
To perform this operation, you must be authenticated by means of one of the following methods:
ApiKeyAuth
</aside>

## Get Upload Details

<a id="opIdGet Upload Details"></a>

> Code samples

```shell
# You can also use wget
curl -X GET /bulk/{stateAbbr}/v2/uploads/{upload_identifier} \
  -H 'Accept: application/json' \
  -H 'From: string' \
  -H 'Ocp-Apim-Subscription-Key: API_KEY'

```

`GET /uploads/{upload_identifier}`

*Get Upload Details*

Get details regarding a bulk upload. Each tenant is allowed a maximum of 120 requests per minute.

<h3 id="get-upload-details-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|From|header|string|true|As in the HTTP/1.1 RFC, used for logging purposes as a means for identifying the source of invalid or unwanted requests. The interpretation of this field is that the request is being performed on behalf of the tenant-affiliated person whose email address (or username) is specified here. It is not used for authentication or authorization.|
|upload_identifier|path|string|true|The Upload ID retrieved from a bulk upload|

<h3 id="get-upload-details-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Bad Request|Inline|
|401|[Unauthorized](https://tools.ietf.org/html/rfc7235#section-3.1)|Access denied|Inline|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|Not Found|Inline|

<h3 id="get-upload-details-responseschema">Response Schema</h3>

Status Code **200**

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» data|object|false|none|The response payload representing upload data.|
|»» upload_identifier|string|true|none|The unique identifier for the upload|
|»» created_at|string|true|none|The timestamp when the requested upload was performed.|
|»» participants_uploaded|integer|false|none|The number or participants uploaded into the piipan|
|»» error_message|string|false|none|Error details as to why an upload failed|
|»» completed_at|string|false|none|The timestamp when the requested upload was completed.|
|»» status|string|true|none|COMPLETE - when successful<br> FAILED - The upload was unsuccessful. The piipan will continue to match against records of active participants from your previous successful upload<br> UPLOADING - The file is uploading to the piipan, or the file has uploaded but is still being validated|

Status Code **400**

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» statusCode|string|false|none|Status of the upload|
|» title|string|false|none|The title of the error|
|» detail|string|false|none|The detailed description of the error|

Status Code **401**

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» statusCode|string|false|none|Status of the upload|
|» message|string|false|none|The message explaining the reason for access denied|

Status Code **404**

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» errors|array|false|none|The array of errors when getting the upload.|
|»» status|string|false|none|Status of the upload|
|»» title|string|false|none|The title of the error|
|»» detail|string|false|none|The detailed description of the error|

### Response Examples

> 200 Response to successful upload

```json
{
  "data": {
    "status": "COMPLETE",
    "upload_identifier": "0x8DA63770FEF1551",
    "created_at": "2022-07-11T19:54:27.903811Z",
    "participants_uploaded": "40",
    "completed_at": "2022-07-11T19:54:28.303961Z"
  }
}
```

> 200 response on an upload that failed

```json
{
  "data": {
    "status": "FAILED",
    "upload_identifier": "0x8DA63770FEF1551",
    "created_at": "2022-07-11T19:54:27.903811Z",
    "error_message": "Exception of type 'CsvHelper.FieldValidationException' was thrown.",
    "completed_at": "2022-07-11T19:54:28.303961Z"
  }
}
```

> 404 Not found exception

```json
{
  "errors": [
    {
      "status": "404",
      "title": "NotFoundException",
      "detail": "not found"
    }
  ]
}
```

> 400 Response

```json
{
  "statusCode": "400",
  "title": "Bad Request Exception",
  "detail": "Request was not valid"
}
```

<aside class="warning">
To perform this operation, you must be authenticated by means of one of the following methods:
ApiKeyAuth
</aside>

