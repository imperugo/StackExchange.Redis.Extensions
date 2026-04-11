# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| 12.x    | Yes       |
| < 12.0  | No        |

Only the latest major version receives security updates. If you are using an older version, please upgrade to v12.

## Reporting a Vulnerability

**Do not open a public issue for security vulnerabilities.**

Please use [GitHub Private Vulnerability Reporting](https://github.com/imperugo/StackExchange.Redis.Extensions/security/advisories/new) to report security issues. This ensures the report is visible only to the repository maintainers until a fix is available.

### What to include

- A description of the vulnerability
- Steps to reproduce
- Affected versions
- Potential impact

### What to expect

- **Acknowledgement** within 48 hours
- **Assessment** of severity and impact within 1 week
- **Fix and release** timeline communicated once the issue is confirmed
- **Credit** in the release notes (unless you prefer to remain anonymous)

### Scope

This policy covers the following packages:

- StackExchange.Redis.Extensions.Core
- StackExchange.Redis.Extensions.AspNetCore
- All serializer packages (System.Text.Json, Newtonsoft, MsgPack, Protobuf, MemoryPack, ServiceStack, Utf8Json)
- All compression packages (GZip, Brotli, LZ4, Snappier, ZstdSharp)
