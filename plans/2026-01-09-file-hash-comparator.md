# Plan: File Hash Comparator

Linked spec:
- specs/2026-01-09-file-hash-comparator.md

## Scope
- .NET 10 WPF-UI app using CommunityToolkit.Mvvm.
- Single-file compare (file vs expected hash, file vs file).
- Batch compare: hash list vs folder, hash list vs hash list.
- Hash algorithms: baseline + extended set per spec, implemented in-code.
- Manual language selector with persisted choice (zh-CN, en-US, ru-RU).

## Out of Scope
- Network features, auto-updates, or cloud sync.
- External hash/crypto libraries beyond approved UI/MVVM packages.

## Steps
1) Bootstrap solution/project for WPF-UI (net10.0-windows) using .slnx, add MVVM toolkit, set up folder structure and localization resources.
2) Implement hashing core: streaming hash service, algorithm registry, and custom implementations for non-built-in algorithms.
3) Implement checksum list parser and comparison engine with detailed result statuses.
4) Build Single Compare UI (two modes) and Batch Compare UI (two modes) with progress/cancel; wire view models and services.
5) Add manual language selector and persistence; ensure resources cover all UI strings.
6) Validate build (and tests if added).

## Validation
```powershell
dotnet --info
dotnet build .\FileHashComparator.slnx
```
