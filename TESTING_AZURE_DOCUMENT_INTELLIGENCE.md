# Testing Azure Document Intelligence

This guide explains how to test the real Azure Document Intelligence service and switch between mock and real implementations.

## Prerequisites

1. ✅ Azure Document Intelligence resource created
2. ✅ Endpoint and Key configured in `appsettings.json`
3. ✅ Application running

## Configuration

### 1. Update appsettings.json

Add the Azure Document Intelligence configuration:

```json
{
  "AzureDocumentIntelligence": {
    "Endpoint": "https://your-resource-name.cognitiveservices.azure.com/",
    "Key": "your-api-key-here",
    "Model": "prebuilt-document",
    "UseMock": false
  }
}
```

**Configuration Options:**
- `Endpoint`: Your Azure Document Intelligence endpoint URL
- `Key`: Your API key (Key 1 or Key 2 from Azure Portal)
- `Model`: The model to use (default: `prebuilt-document`)
  - `prebuilt-document`: General document analysis (recommended)
  - `prebuilt-layout`: Layout analysis
  - `prebuilt-read`: OCR reading
- `UseMock`: Set to `false` to use real Azure service, `true` for mock

### 2. Switch Between Mock and Real Service

**Option A: Using Configuration (Recommended)**

Set `UseMock` in appsettings:
- `"UseMock": true` → Uses MockFormExtractorService (no Azure calls)
- `"UseMock": false` → Uses AzureFormExtractorService (real Azure API)

**Option B: Using Environment Variables**

For different environments:

```bash
# Development - use mock
export AzureDocumentIntelligence__UseMock=true

# Production - use real service
export AzureDocumentIntelligence__UseMock=false
```

## Testing Steps

### Step 1: Prepare Test PDFs

Create or download sample PDF forms:
- Contact form
- Application form
- Registration form
- Survey form

Place them in a test directory, e.g., `/Users/pete/Documents/test-forms/`

### Step 2: Test with Mock Service (Verify Setup)

1. Ensure `"UseMock": true` in appsettings.json
2. Run the application
3. Upload a PDF via the API:

```bash
curl -X POST https://localhost:57679/api/forms/upload-pdf \
  -F "PdfFile=@/path/to/your/test-form.pdf" \
  -k
```

4. Verify you get a response with mock data

### Step 3: Test with Real Azure Service

1. Change `"UseMock": false` in appsettings.json
2. Restart the application
3. Upload the same PDF:

```bash
curl -X POST https://localhost:57679/api/forms/upload-pdf \
  -F "PdfFile=@/path/to/your/test-form.pdf" \
  -k
```

4. Verify you get real extracted fields from Azure

### Step 4: Compare Results

**Mock Service Response:**
```json
{
  "formId": "guid-here",
  "fileName": "contact-form.pdf",
  "formType": "contact",
  "fieldCount": 5,
  "tableCount": 0,
  "warnings": [],
  "requiresManualReview": false
}
```

**Real Azure Service Response:**
```json
{
  "formId": "guid-here",
  "fileName": "contact-form.pdf",
  "formType": "contact",
  "fieldCount": 12,
  "tableCount": 1,
  "warnings": ["Multi-page document detected (2 pages)"],
  "requiresManualReview": true
}
```

## Using Swagger UI

1. Navigate to https://localhost:57679/swagger
2. Find the `POST /api/forms/upload-pdf` endpoint
3. Click "Try it out"
4. Upload your test PDF
5. Execute and view the response

## Using Postman

1. Create a new POST request: `https://localhost:57679/api/forms/upload-pdf`
2. Go to "Body" → "form-data"
3. Add key `PdfFile` (type: File)
4. Select your PDF file
5. Send request

## Viewing Created Forms

### Get Form by ID
```bash
curl https://localhost:57679/api/forms/{form-id} -k
```

### List All Forms
```bash
curl https://localhost:57679/api/forms -k
```

### Query the Database Directly
```bash
sqlite3 src/FormDesignerAPI.Web/database.sqlite "SELECT Id, Name, Origin_Type FROM Forms;"
```

## What Azure Document Intelligence Extracts

The real Azure service will extract:

1. **Key-Value Pairs**: Field labels and their values
   - Example: "First Name" → "John"

2. **Tables**: Structured data in tables
   - Headers, rows, columns
   - Cell positions and content

3. **Form Fields**: Detected form elements
   - Text fields, checkboxes, signatures

4. **Layout Information**: Document structure
   - Pages, sections, reading order

## Expected Differences: Mock vs Real

| Feature | Mock Service | Real Azure Service |
|---------|-------------|-------------------|
| Fields extracted | Template-based (5-8 fields) | Actual PDF content (varies) |
| Field types | Predefined types | Inferred from content |
| Tables | None | All tables extracted |
| Validation | None | Based on content |
| Performance | Instant | 1-3 seconds per page |
| Cost | Free | Per page (see Azure pricing) |

## Troubleshooting

### Error: "Azure Document Intelligence Endpoint not configured"
- Check appsettings.json has correct `AzureDocumentIntelligence` section
- Verify endpoint URL format: `https://your-name.cognitiveservices.azure.com/`

### Error: "401 Unauthorized"
- Verify API key is correct
- Check key hasn't been regenerated in Azure Portal
- Try using Key 2 instead of Key 1

### Error: "403 Forbidden"
- Check Azure resource is in the same subscription
- Verify pricing tier allows API calls (Free tier: 500 pages/month)

### Error: "Operation timed out"
- Large PDFs (>10 pages) may take longer
- Check network connectivity to Azure
- Verify region is accessible

### No Fields Extracted
- PDF may be image-based (scanned) - Azure will OCR it
- Form may be too complex
- Try a simpler test PDF first

## Monitoring Azure Usage

### View API Calls in Azure Portal
1. Go to your Document Intelligence resource
2. Click "Metrics" in the left menu
3. View:
   - Total calls
   - Success rate
   - Average latency
   - Throttled requests

### Check Costs
1. Azure Portal → Cost Management
2. Filter by Document Intelligence resource
3. View daily/monthly costs

## Best Practices

1. **Start with Mock**: Test your workflow with mock service first
2. **Small PDFs First**: Test with 1-2 page forms before large documents
3. **Rate Limiting**: Free tier limited to 20 calls/minute
4. **Error Handling**: Always check response for warnings
5. **Cost Management**: Monitor usage to avoid unexpected charges

## Next Steps

After successful testing:
1. ✅ Verify form structure is correct
2. ✅ Check field mappings are accurate
3. ✅ Test code generation with extracted forms
4. ✅ Deploy to production with real Azure service
