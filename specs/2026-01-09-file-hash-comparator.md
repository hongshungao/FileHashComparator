# File Hash Comparator (WPF-UI, MVVM, .NET 10)

## Overview
Build a Windows desktop app to compute and compare file hashes. It must support:
- Single-file hash verification (compute vs expected hash).
- Multi-file hash comparison using checksum list files (two modes).
- Common algorithms including MD2, MD4, MD5, SHA1, SHA256, SHA512, CRC32.
- Multilingual UI (at least zh-CN, en-US, ru-RU).

Tech stack target: .NET 10, WPF-UI, MVVM (CommunityToolkit.Mvvm).

## Goals
- Simple, offline verification of file integrity.
- Fast, streaming hash computation for large files.
- Clear, actionable results (match, mismatch, missing, extra, error).
- Support common checksum file formats.

## Non-Goals
- No cloud sync or network features.
- No encryption or signing features.
- No automatic package installation by the agent.

## User Flows
### Single File
Two modes:
1) File vs expected hash
   - Select a file (browse or drag/drop).
   - Choose hash algorithm.
   - Paste expected hash (optional).
   - Compute and show:
     - computed hash
     - match/mismatch/invalid expected hash
2) File vs file
   - Select File A and File B.
   - Choose hash algorithm.
   - Compute and show:
     - File A hash value
     - File B hash value
     - match/mismatch

### Batch Compare
Two modes:
1) Hash list vs folder
   - Select a checksum list file and a root folder.
   - For each entry, compute hash of the referenced file and compare.
2) Hash list vs hash list
   - Select two checksum list files.
   - Compare entries by normalized relative path.

## Supported Hash List Formats
Parse common checksum formats:
- `HASH <space> [*]path`
  - e.g., `d41d8cd98f00b204e9800998ecf8427e *file.txt`
- `HASH <space><space> path` (two spaces)
- Ignore empty lines and lines starting with `#`.
- Trim surrounding whitespace.
- Hash is hex and case-insensitive.

If a line does not match any supported format, mark it as a parse error entry.

## Hash Algorithms
Minimum set:
- MD2
- MD4
- MD5
- SHA1
- SHA256
- SHA512
- CRC32

Extended set (as many as feasible):
Cryptographic:
- SHA224
- SHA384
- SHA512/224
- SHA512/256
- SHA3-224
- SHA3-256
- SHA3-384
- SHA3-512
- SHAKE128
- SHAKE256
- RIPEMD160
- Whirlpool
- BLAKE2b
- BLAKE2s
- BLAKE3

Non-cryptographic / fast checksums:
- Adler32
- CRC32C
- CRC64 (ECMA)
- XXHash32
- XXHash64
- Murmur3 (32/128)

Implementation notes:
- Use built-in .NET algorithms where available.
- Implement MD2, MD4, CRC32 in-code to avoid extra dependencies unless approved.
- Extended algorithms should be implemented in-code (no external dependencies).

## UI/UX
Main window with two tabs/pages:
- Single Compare
- Batch Compare

Common UI elements:
- File/folder pickers and drag/drop.
- Algorithm selection dropdown.
- Results panel with status and copy buttons.
- Progress indicator and cancel for batch operations.

Batch results table columns:
- Path
- Expected Hash
- Actual Hash
- Status (Match/Mismatch/Missing/Extra/Unreadable/ParseError)

## Error Handling
- Missing file: status "Missing".
- Access denied/unreadable: status "Unreadable".
- Invalid expected hash length/characters: status "InvalidExpected".
- Parse error line: status "ParseError".
- Always show errors in results; do not crash.

## Performance
- Stream file hashing (no full-file load).
- Async operations with cancellation.
- UI remains responsive during hashing.

## MVVM Structure (High-Level)
- ViewModels:
  - MainViewModel (navigation)
  - SingleCompareViewModel
  - BatchCompareViewModel
- Services:
  - IHashService (compute hash)
  - IHashListParser (parse checksum lists)
  - IFileDialogService (open file/folder)
- Models:
  - HashAlgorithmType
  - HashResult
  - CompareEntry

## Open Questions
- Confirm whether both batch modes are required or only one.
- Confirm localization behavior (follow OS language, or manual selector with persisted choice).
