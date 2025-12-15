# Azure Document Intelligence Setup Guide

## Overview
This guide walks you through setting up Azure Document Intelligence for the FormDesigner application to extract form fields from PDF documents.

## Prerequisites
- Azure subscription
- Azure CLI installed (optional but recommended)
- .NET 9 SDK

## Step 1: Create Azure Document Intelligence Resource

### Option A: Using Azure Portal
1. Go to [Azure Portal](https://portal.azure.com)
2. Click **"Create a resource"**
3. Search for **"Azure AI Document Intelligence"** (formerly Form Recognizer)
4. Click **"Create"**
5. Fill in the details:
   - **Subscription**: Select your subscription
   - **Resource Group**: Create new or use existing
   - **Region**: Choose closest region (e.g., East US, West US 2)
   - **Name**: `formdesigner-docintel` (or your preferred name)
   - **Pricing tier**:
     - **Free F0**: 500 pages/month, 20 calls/min (good for development)
     - **Standard S0**: Pay-as-you-go (for production)
6. Click **"Review + create"** then **"Create"**
7. Wait for deployment to complete

### Option B: Using Azure CLI
```bash
# Login to Azure
az login

# Create resource group
az group create --name formdesigner-rg --location eastus

# Create Document Intelligence resource
az cognitiveservices account create \
  --name formdesigner-docintel \
  --resource-group formdesigner-rg \
  --kind FormRecognizer \
  --sku F0 \
  --location eastus \
  --yes

# Get the endpoint and key
az cognitiveservices account show \
  --name formdesigner-docintel \
  --resource-group formdesigner-rg \
  --query "properties.endpoint" \
  --output tsv

az cognitiveservices account keys list \
  --name formdesigner-docintel \
  --resource-group formdesigner-rg \
  --query "key1" \
  --output tsv
```

## Step 2: Get Your Credentials

### From Azure Portal:
1. Go to your Document Intelligence resource
2. Click **"Keys and Endpoint"** in the left menu
3. Copy:
   - **KEY 1** (or KEY 2)
   - **Endpoint** (e.g., `https://formdesigner-docintel.cognitiveservices.azure.com/`)

## Step 3: Configure Application

### Update `appsettings.json`:
```json
{
  "AzureDocumentIntelligence": {
    "Endpoint": "https://your-resource-name.cognitiveservices.azure.com/",
    "Key": "your-api-key-here",
    "Model": "prebuilt-document"
  }
}
```

### Update `appsettings.Development.json` (for local development):
```json
{
  "AzureDocumentIntelligence": {
    "Endpoint": "https://your-resource-name.cognitiveservices.azure.com/",
    "Key": "your-development-key-here",
    "Model": "prebuilt-document"
  }
}
```

### For Production - Use Azure Key Vault (Recommended):
```json
{
  "AzureDocumentIntelligence": {
    "Endpoint": "https://your-resource-name.cognitiveservices.azure.com/",
    "KeyVaultName": "your-keyvault-name",
    "KeySecretName": "DocumentIntelligenceKey",
    "Model": "prebuilt-document"
  }
}
```

## Step 4: Install NuGet Package

The package is already included in the project dependencies, but if you need to add it:

```bash
cd src/FormDesignerAPI.Infrastructure
dotnet add package Azure.AI.FormRecognizer --version 4.1.0
```

## Step 5: Test the Connection

Once configured, you can test the connection using the mock implementation first, then switch to the real Azure service.

### Testing with Mock (Current Implementation):
The project includes a `MockFormExtractorService` that simulates Document Intelligence responses without requiring Azure credentials. This is perfect for development.

### Testing with Real Azure Service:
1. Ensure your `appsettings.json` has the correct endpoint and key
2. Comment out the mock service registration
3. Uncomment the real service registration in `InfrastructureServiceExtensions.cs`
4. Upload a PDF through the API
5. Check logs for extraction results

## Available Document Intelligence Models

### Prebuilt Models (No Training Required):
- **`prebuilt-document`**: General document analysis (recommended for forms)
- **`prebuilt-layout`**: Extract text, tables, and structure
- **`prebuilt-invoice`**: Specialized for invoices
- **`prebuilt-receipt`**: Specialized for receipts
- **`prebuilt-businessCard`**: Business cards
- **`prebuilt-idDocument`**: ID documents (driver's license, passport)

### Custom Models (Requires Training):
If you have specific form templates, you can train custom models:
1. Prepare 5+ sample PDFs
2. Use Document Intelligence Studio to label and train
3. Get custom model ID
4. Update `appsettings.json` with your model ID

## Pricing Information

### Free Tier (F0):
- 500 pages/month
- 20 transactions/minute
- Good for development and testing
- No SLA

### Standard Tier (S0):
- **Read API**: $1.50 per 1,000 pages
- **Prebuilt Models**: $10 per 1,000 pages
- **Custom Models**: $40 per 1,000 pages (extraction)
- Training: $10 per 1,000 pages
- Unlimited throughput
- 99.9% SLA

[Full Pricing Details](https://azure.microsoft.com/en-us/pricing/details/form-recognizer/)

## Document Intelligence Studio

For testing and model training, use the web-based studio:
- URL: https://formrecognizer.appliedai.azure.com/
- Sign in with Azure credentials
- Test different models
- Train custom models
- View extraction results in real-time

## Security Best Practices

1. **Never commit API keys to source control**
   - Use user secrets for development
   - Use Azure Key Vault for production

2. **Rotate keys regularly**
   - Document Intelligence provides 2 keys
   - Rotate without downtime

3. **Use Managed Identity in Azure**
   - If hosting in Azure App Service/Container Apps
   - No keys needed - uses Azure AD

4. **Restrict network access**
   - Configure firewall rules
   - Use Private Endpoints for production

## Troubleshooting

### "Unauthorized" Error:
- Check endpoint URL matches your resource
- Verify API key is correct
- Ensure key isn't expired

### "Rate Limit Exceeded":
- Upgrade from Free to Standard tier
- Implement retry logic with exponential backoff

### Poor Extraction Quality:
- Try different prebuilt models
- Consider training a custom model
- Ensure PDFs are high quality (not scanned images)
- Use OCR-ready PDFs when possible

### Timeout Errors:
- Large PDFs may take time to process
- Use async/await properly
- Consider increasing timeout settings

## Next Steps

1. Get your Azure credentials
2. Update `appsettings.json`
3. Test with the mock service first
4. Switch to real Azure service
5. Upload sample PDFs
6. Review extraction results
7. Fine-tune field mapping as needed

## Support Resources

- [Azure Document Intelligence Documentation](https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/)
- [SDK Reference](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.formrecognizer-readme)
- [Code Samples](https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/formrecognizer/Azure.AI.FormRecognizer)
- [Studio](https://formrecognizer.appliedai.azure.com/)
