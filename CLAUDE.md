# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build the entire solution
dotnet build PaymentProcessorApp.sln

# Build individual projects
dotnet build PaymentProcessorApp/PaymentProcessorApp.csproj
dotnet build CheckReader/CheckReader.csproj

# Run the application
dotnet run --project PaymentProcessorApp/PaymentProcessorApp.csproj

# Docker build
docker build -t payment-processor -f PaymentProcessorApp/Dockerfile .

# Run all tests
dotnet test CheckReader.Tests/CheckReader.Tests.csproj

# Run a single test by name
dotnet test CheckReader.Tests/CheckReader.Tests.csproj --filter "FullyQualifiedName~AmountParserTests.ExtractAmount_WithDollarSignAndCents"
```

## Architecture

This is a .NET 10.0 solution for processing check images using OCR. It consists of two projects:

**PaymentProcessorApp** - Console application entry point

**CheckReader** - Class library that performs OCR-based check reading using Tesseract
- `Services/CheckReader.cs` - Core service that orchestrates OCR extraction via Tesseract engine and delegates parsing to specialized parsers
- `Domain/Check.cs` - Domain model with value objects: `Check`, `Amount`, `Micr`, `Payee`
- `Parsers/` - Regex-based parsers for extracting specific fields from OCR text:
  - `MicrParser` - Extracts routing number (9 digits), account number (8-17 digits), check number from MICR line
  - `AmountParser` - Extracts dollar amounts in various formats
  - `DateParser` - Extracts dates in common check formats
  - `PayeeParser` - Extracts payee name from "Pay to the order of" line

## Key Dependencies

- **Tesseract 5.2.0** - OCR engine for reading check images
- Tessdata files must be present in `CheckReader/tessdata/` directory (copied to output on build)