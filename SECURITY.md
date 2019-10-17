# Security Policy

## Reporting a Vulnerability

**IMPORTANT**: Do NOT open a public GitHub issue to report a security vulnerability. Public issues can expose the vulnerability to malicious actors.

### Private Vulnerability Reporting (Recommended)

Use GitHub's Private Vulnerability Reporting feature:

1. Go to [Security Advisories](https://github.com/sarmkadan/ffmpeg-dotnet-wrapper/security/advisories/new)
2. Click "Report a vulnerability"
3. Fill out the vulnerability details
4. Submit privately

The maintainers will be notified immediately and you'll be able to discuss the issue in a private forum before public disclosure.

### Email Reporting

Alternatively, email security details to: **rutova2@gmail.com**

Include:
- Description of the vulnerability
- Steps to reproduce (if applicable)
- Affected version(s)
- Suggested fix (if you have one)

## Response Timeline

- **Acknowledgment**: Within 48 hours
- **Assessment**: Within 1 week
- **Fix & Release**: Prioritized based on severity

## Supported Versions

| Version | Status | Supported |
|---------|--------|-----------|
| 1.x     | Latest | ✅ Yes    |
| < 1.0   | Legacy | ❌ No     |

Only the latest version (1.x) receives security updates. We strongly recommend upgrading to the latest version.

## Security Best Practices

When using FFmpeg .NET Wrapper:

1. **Input Validation**: Always validate file paths and user input
2. **FFmpeg Updates**: Keep FFmpeg binary updated to the latest version
3. **Permissions**: Run with minimal required file system permissions
4. **Timeouts**: Configure appropriate timeouts for long-running operations
5. **Resource Limits**: Be aware of disk space and CPU usage limits

## Dependency Security

This library has minimal dependencies (only `Microsoft.Extensions.*`). We:
- Monitor dependencies for known vulnerabilities
- Keep dependencies updated
- Avoid unnecessary external packages

---

Thank you for helping keep FFmpeg .NET Wrapper secure! 🔒
