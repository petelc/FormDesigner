## **Usage Instructions**

### For Windows:
```powershell
# Save the PowerShell script as generate-docs.ps1
# Run it:
.\generate-docs.ps1

# After completion, create ZIP:
Compress-Archive -Path FormGenAI-Migration-Guide -DestinationPath FormGenAI-Migration-Guide.zip
```

### For Mac/Linux:
```bash
# Save the Bash script as generate-docs.sh
# Make it executable:
chmod +x generate-docs.sh

# Run it:
./generate-docs.sh

# After completion, create ZIP:
zip -r FormGenAI-Migration-Guide.zip FormGenAI-Migration-Guide
```

---

## What the Scripts Do

1. **Create directory structure** with all necessary folders
2. **Generate README.md** with table of contents
3. **Create 00-OVERVIEW.md** with architecture and prerequisites
4. **Generate all 9 phase documents** (01-PHASE-1 through 09-PHASE-9)
5. **Create 3 appendices** (Code Examples, Troubleshooting, Glossary)
6. **Add diagram templates** in Mermaid format
7. **Include useful templates** (commit messages, checklists)

The generated files include:
- ✅ Proper markdown formatting
- ✅ Code blocks with syntax highlighting
- ✅ Tables and checklists
- ✅ Cross-references between documents
- ✅ Git commit message templates
- ✅ Troubleshooting guides
- ✅ Complete glossary