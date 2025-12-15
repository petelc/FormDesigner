# Quick Start: Testing Azure Document Intelligence

## TL;DR - How to Switch Services

### Using Mock Service (Default - No Azure Required)
```json
// appsettings.json
"AzureDocumentIntelligence": {
  "UseMock": true
}
```

### Using Real Azure Service
```json
// appsettings.json
"AzureDocumentIntelligence": {
  "Endpoint": "https://your-actual-resource.cognitiveservices.azure.com/",
  "Key": "paste-your-actual-key-here",
  "Model": "prebuilt-document",
  "UseMock": false
}
```

## Step-by-Step Testing

### 1. Test with Mock (Currently Active)
```bash
# Application is already running with UseMock: true
# Upload a test PDF
curl -X POST https://localhost:57679/api/forms/upload-pdf \
  -F "PdfFile=@/path/to/test.pdf" \
  -k
```

You'll get instant results from the mock service (no Azure calls).

### 2. Test with Real Azure

**A. Update appsettings.json:**
```json
"AzureDocumentIntelligence": {
  "Endpoint": "https://YOUR-RESOURCE-NAME.cognitiveservices.azure.com/",
  "Key": "YOUR-ACTUAL-KEY",
  "Model": "prebuilt-document",
  "UseMock": false
}
```

**B. Restart the application:**
```bash
# Stop the current app (Ctrl+C)
cd src/FormDesignerAPI.Web
dotnet run
```

**C. Look for this log message:**
```
[INFO] Using AzureFormExtractorService (real Azure Document Intelligence)
```

**D. Upload the same PDF:**
```bash
curl -X POST https://localhost:57679/api/forms/upload-pdf \
  -F "PdfFile=@/path/to/test.pdf" \
  -k
```

Now you'll get real extraction from Azure!

## What You'll See

### Mock Service Logs:
```
[INFO] Using MockFormExtractorService (no Azure API calls)
[INFO] Mock: Detecting form type for test.pdf
[INFO] Mock: Detected form type 'contact' based on filename
```

### Real Azure Logs:
```
[INFO] Using AzureFormExtractorService (real Azure Document Intelligence)
[INFO] AzureFormExtractorService initialized with endpoint: https://...
[INFO] Detecting form type for: /tmp/test.pdf
[INFO] Extracting form structure from /tmp/test.pdf as contact
[INFO] Extracted 12 fields and 1 tables from /tmp/test.pdf
```

## Quick Tests

### Test 1: Verify Mock Works
```bash
# Should return instantly with template fields
curl -X POST https://localhost:57679/api/forms/upload-pdf \
  -F "PdfFile=@contact-form.pdf" \
  -k | jq
```

### Test 2: Switch to Azure and Compare
```bash
# 1. Change UseMock to false
# 2. Restart app
# 3. Upload same file
curl -X POST https://localhost:57679/api/forms/upload-pdf \
  -F "PdfFile=@contact-form.pdf" \
  -k | jq
```

### Test 3: View Created Form
```bash
# Get the formId from upload response
curl https://localhost:57679/api/forms/{formId} -k | jq
```

## Troubleshooting

### "Azure Document Intelligence Endpoint not configured"
- You forgot to replace `your-resource-name` with your actual Azure resource name
- Check Azure Portal → Your Resource → Keys and Endpoint

### "401 Unauthorized"
- Wrong API key
- Copy the key EXACTLY from Azure Portal (no extra spaces)

### Still seeing mock data after changing UseMock
- Did you restart the application?
- Check the startup logs for which service is registered

### Application won't start
- Missing NuGet package? Run: `dotnet restore`
- Check appsettings.json is valid JSON (no trailing commas)

## Pro Tips

1. **Start with Mock**: Always test your workflow with mock first
2. **Check Logs**: The startup log tells you which service is active
3. **Small PDFs**: Test with 1-page forms initially
4. **Cost Awareness**: Free tier = 500 pages/month
5. **Save Test Files**: Keep a set of test PDFs for regression testing

## Example Response Comparison

### Mock Response:
```json
{
  "formId": "abc-123",
  "fieldCount": 5,
  "formType": "contact",
  "warnings": []
}
```

### Azure Response:
```json
{
  "formId": "xyz-789",
  "fieldCount": 12,
  "formType": "contact",
  "warnings": ["Multi-page document detected (2 pages)"],
  "requiresManualReview": true
}
```

Notice: Azure extracts MORE fields from the actual PDF content!

## Next Steps

1. ✅ Test with mock (works now)
2. ✅ Get Azure credentials from Portal
3. ✅ Update appsettings.json with real values
4. ✅ Set `UseMock: false`
5. ✅ Restart and test with real Azure
6. ✅ Compare results
7. ✅ Use extracted data for code generation
