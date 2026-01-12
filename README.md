# 文件哈希对比器 / File Hash Comparator / Сравнение хэшей файлов

## 中文
用于计算并对比文件哈希的桌面应用（WPF）。
支持单文件与批量比对，并提供多语言界面。

功能概览：
- 单文件 vs 期望值、文件 vs 文件比对。
- 批量列表 vs 文件夹、列表 vs 列表比对。

运行方式：
```powershell
dotnet build .\src\FileHashComparator.App\FileHashComparator.App.csproj
dotnet run --project .\src\FileHashComparator.App\FileHashComparator.App.csproj
```
说明：部分算法由 .NET 运行时按名称提供。
若当前环境不支持，应用会自动隐藏该算法。

## English
A WPF desktop app for computing and comparing file hashes.
Supports single-file and batch comparisons with a localized UI.

Features:
- File vs expected hash, file vs file.
- List vs folder, list vs list batch comparisons.

Run locally:
```powershell
dotnet build .\src\FileHashComparator.App\FileHashComparator.App.csproj
dotnet run --project .\src\FileHashComparator.App\FileHashComparator.App.csproj
```
Note: Some algorithms rely on runtime-provided implementations.
They may be unavailable on a given system; the app hides unsupported options.

## Русский
Настольное WPF-приложение для вычисления и сравнения хэшей файлов.
Поддерживает одиночные и пакетные сравнения, есть локализация.

Возможности:
- Файл vs ожидаемый хэш, файл vs файл.
- Список vs папка, список vs список.

Запуск:
```powershell
dotnet build .\src\FileHashComparator.App\FileHashComparator.App.csproj
dotnet run --project .\src\FileHashComparator.App\FileHashComparator.App.csproj
```
Примечание: часть алгоритмов зависит от реализаций .NET.
На некоторых системах они недоступны; приложение скрывает неподдерживаемые варианты.

## 支持的算法 / Supported Algorithms / Поддерживаемые алгоритмы
- 加密 / Cryptographic / Криптографические:
  - MD2
  - MD4
  - MD5
  - SHA-1
  - SHA-224
  - SHA-256
  - SHA-384
  - SHA-512
  - SHA-512/224
  - SHA-512/256
  - SHA3-224
  - SHA3-256
  - SHA3-384
  - SHA3-512
  - SHAKE128
  - SHAKE256
  - RIPEMD-160
  - Whirlpool
  - BLAKE2b
  - BLAKE2s
  - BLAKE3
- 非加密 / Non-cryptographic / Некриптографические:
  - CRC32
  - CRC32C
  - CRC64-ECMA
  - Adler32
  - XXHash32
  - XXHash64
  - Murmur3-32
  - Murmur3-128
