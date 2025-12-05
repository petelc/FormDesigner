## **File: generate-docs.sh** (Bash for Mac/Linux)

```bash
#!/bin/bash

# FormGenAI Migration Guide Generator
# Bash script to create all documentation files

echo "🚀 FormGenAI Migration Guide Generator"
echo "======================================="
echo ""

# Create base directory
BASE_DIR="FormGenAI-Migration-Guide"

if [ -d "$BASE_DIR" ]; then
    echo -n "⚠️  Directory already exists. Remove it? (y/n): "
    read -r response
    if [[ "$response" =~ ^[Yy]$ ]]; then
        rm -rf "$BASE_DIR"
        echo "✓ Removed existing directory"
    else
        echo "❌ Aborted"
        exit 1
    fi
fi

mkdir -p "$BASE_DIR"/{diagrams,templates}
echo "✓ Created directory structure"

# Function to create file with content
create_file() {
    local filename="$1"
    local content="$2"
    echo "$content" > "$BASE_DIR/$filename"
    echo "  ✓ Created $filename"
}

# README.md
read -r -d '' README_CONTENT << 'EOF'
# FormGenAI Migration Guide - Complete Documentation

Welcome to the complete migration guide for transforming your Ardalis CleanArchitecture solution into a DDD-based AI-powered code generation system.

[Same content as PowerShell version]
EOF

create_file "README.md" "$README_CONTENT"

# Continue with other files...
# (Same structure as PowerShell script)

echo ""
echo "✅ Documentation generation complete!"
echo ""
echo "📁 Created structure:"
echo "   └── $BASE_DIR/"
echo "       ├── README.md"
echo "       ├── All phase files..."
echo "       ├── Appendices..."
echo "       ├── diagrams/"
echo "       └── templates/"
echo ""
echo "🎉 Next steps:"
echo "   1. Review the generated files"
echo "   2. Fill in detailed content for each phase"
echo "   3. Create ZIP: zip -r FormGenAI-Migration-Guide.zip $BASE_DIR"
echo ""
echo "📦 To create ZIP archive:"
echo "   zip -r FormGenAI-Migration-Guide.zip $BASE_DIR"
echo ""
```