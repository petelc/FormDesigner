# Azure AI Code Generation - Migration Guide

This directory contains all documentation for integrating Azure AI-powered form-to-code generation into the FormDesignerAPI Clean Architecture solution.

## 📚 Documentation Structure

### Quick Start
1. **Phase1_Preparation.md** - Set up NuGet packages and directory structure (15 min)
2. **Phase2_CoreLayer.md** - Add domain entities and interfaces (20 min)
3. **Phase3_UseCasesLayer.md** - Implement application logic (25 min)
4. **Phase4_InfrastructureLayer_Part1.md** - Azure AI services implementation (40 min)

### Comprehensive Guides
- **Migration_Guide.md** - Complete step-by-step migration guide
- **AzureAI_CodeGeneration_Refactoring_Guide.md** - Architecture and design decisions

## 🎯 What This Migration Adds

- **Azure Document Intelligence** integration for PDF form analysis
- **Azure OpenAI** integration for code generation
- Support for **AcroForm, XFA, and Hybrid** PDF forms
- **Batch processing** capabilities for 1000+ forms
- **Clean Architecture** compliance

## 🚀 Getting Started

1. Read `Phase1_Preparation.md`
2. Follow each phase sequentially
3. Test after each phase
4. Refer to comprehensive guides for detailed explanations

## ⚙️ Prerequisites

- Azure Document Intelligence resource
- Azure OpenAI resource
- .NET 8 SDK
- Visual Studio 2022 or VS Code

## 📋 Migration Phases

| Phase | Duration | Description |
|-------|----------|-------------|
| Phase 1 | 15 min | Install packages, create directories |
| Phase 2 | 20 min | Core domain entities and interfaces |
| Phase 3 | 25 min | Use cases and DTOs |
| Phase 4 | 40 min | Azure AI service implementations |
| Testing | 30 min | Verify and test integration |

**Total Time:** ~2.5 hours

## 🆘 Getting Help

If you encounter issues:
1. Check the troubleshooting section in Migration_Guide.md
2. Verify Azure credentials in appsettings.json
3. Ensure all NuGet packages are restored
4. Review logs for detailed error information

## 📝 Notes

- Each phase builds on the previous one
- Test after completing each phase
- Keep Azure API keys secure (use User Secrets or Azure Key Vault)
- The migration preserves all existing functionality

Good luck with your migration! 🎉
