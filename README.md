# NinjaScriptAudit

A command-line tool for auditing scripts in your NinjaOne RMM instance by searching for specific patterns using regular expressions.

## Overview

NinjaScriptAudit allows you to search through all automation scripts in your NinjaOne (formerly NinjaRMM) instance using regular expressions. This is useful for:

- **Security auditing**: Find scripts containing hardcoded credentials or sensitive information
- **Code review**: Identify scripts using deprecated functions or patterns
- **Compliance**: Ensure scripts follow organizational standards
- **Maintenance**: Locate scripts that need updates or modifications

The tool uses NinjaOne's OAuth 2.0 API to authenticate and retrieve script metadata, then fetches the full script content to perform pattern matching.

## Features

- OAuth 2.0 authentication with NinjaOne
- Regex-based pattern matching across all scripts
- Supports all script types (PowerShell, Bash, etc.)
- Interactive browser-based authorization flow
- Native AOT compilation for fast startup and low memory usage

## Prerequisites

- .NET 10.0 SDK or runtime
- A NinjaOne account with API access
- OAuth credentials (client ID, client secret) from NinjaOne
- Access to the NinjaOne instance you want to audit

### Getting NinjaOne OAuth Credentials

1. Log in to your NinjaOne instance
2. Navigate to **Administration** → **Apps** → **API**
3. Click **Add** to create a new API client
4. Configure the OAuth client:
   - **Allowed Scopes**: Select the required scopes (e.g., `monitoring`, `management`, `control`, `offline_access`)
   - **Redirect URIs**: Add `http://localhost:8080/` (or your custom redirect URI)
5. Save the **Client ID** and **Client Secret** for use with this tool

## Installation

### Option 1: Build from Source

```bash
git clone https://github.com/ProVal-Tech/NinjaScriptAudit.git
cd NinjaScriptAudit
dotnet build
```

### Option 2: Publish as Native AOT

For better performance and distribution:

```bash
dotnet publish -c Release -r win-x64
# Or for other platforms:
# dotnet publish -c Release -r linux-x64
# dotnet publish -c Release -r osx-x64
```

The compiled executable will be in `NinjaScriptAudit/bin/Release/net10.0/{runtime}/publish/`

## Usage

### Basic Command

```bash
NinjaScriptAudit --instance <instance> --client-id <client-id> --client-secret <client-secret> --scope <scope> --search-regex <pattern>
```

### Parameters

| Parameter | Short | Required | Default | Description |
|-----------|-------|----------|---------|-------------|
| `--instance` | `-i` | Yes | - | Your NinjaOne instance URL (e.g., `provaltech.rmmservice.com`) |
| `--client-id` | `-c` | Yes | - | OAuth client ID from NinjaOne |
| `--client-secret` | `-s` | Yes | - | OAuth client secret from NinjaOne |
| `--scope` | `-o` | Yes | - | OAuth scopes (space-separated, e.g., `offline_access monitoring management control`) |
| `--redirect-uri` | `-r` | No | `http://localhost:8080/` | Redirect URI for OAuth callback |
| `--search-regex` | `-x` | Yes | - | Regular expression pattern to search for in scripts |

### Examples

#### Search for hardcoded passwords

```bash
NinjaScriptAudit \
  --instance provaltech.rmmservice.com \
  --client-id your-client-id \
  --client-secret your-client-secret \
  --scope "offline_access monitoring management control" \
  --search-regex "password\s*=\s*['\"][^'\"]+['\"]"
```

#### Find scripts using deprecated cmdlets

```bash
NinjaScriptAudit \
  --instance provaltech.rmmservice.com \
  --client-id your-client-id \
  --client-secret your-client-secret \
  --scope "offline_access monitoring management control" \
  --search-regex "Invoke-WebRequest|wget"
```

#### Locate scripts with specific variable names

```bash
NinjaScriptAudit \
  --instance provaltech.rmmservice.com \
  --client-id your-client-id \
  --client-secret your-client-secret \
  --scope "offline_access monitoring management control" \
  --search-regex "\$apiKey|\$secretKey"
```

## Authentication Flow

1. The tool starts a local HTTP listener on the specified redirect URI
2. A browser authorization URL is displayed in the console
3. Click the link to open your browser and authorize the application
4. After authorization, you'll be redirected to a success page
5. Follow the on-screen instructions to copy your `sessionKey` cookie from browser DevTools:
   - Press **F12** to open DevTools
   - Go to **Application** (Chrome/Edge) or **Storage** (Firefox)
   - Expand **Cookies** and select your NinjaOne URL
   - Copy the `sessionKey` value
6. Paste the session key into the console when prompted
7. The tool will fetch and search through all scripts

### Why is the sessionKey needed?

The `sessionKey` cookie is required because the NinjaOne API's OAuth token doesn't provide direct access to the full script content endpoint. The session key allows the tool to fetch the complete script code from the web interface endpoint.

## Output

The tool outputs matching scripts to the console:

```
Match found in script ID 12345, Name: Server Backup Script
Match found in script ID 67890, Name: User Provisioning
```

## Security Considerations

- **Credentials**: Never hardcode credentials in scripts. Use secure storage methods or environment variables.
- **Session Key**: The session key is equivalent to being logged in to NinjaOne. Handle it securely and don't share it.
- **OAuth Secrets**: Keep your client secret secure and don't commit it to version control.
- **Regex Patterns**: Be mindful of regex complexity to avoid performance issues with large script collections.

## Technical Details

- **Language**: C# (.NET 10.0)
- **Target**: Console application with Native AOT compilation support
- **Dependencies**:
  - `System.CommandLine` 2.0.0 - Command-line parsing
  - Built-in System.Text.Json with source generation for optimal performance

## Troubleshooting

### "Failed to obtain access token"

- Verify your client ID and client secret are correct
- Ensure the redirect URI matches what's configured in NinjaOne
- Check that the requested scopes are allowed for your OAuth client

### "Failed to retrieve scripts"

- Verify your OAuth client has the necessary permissions
- Ensure the `monitoring` or `management` scope is included

### Browser doesn't open automatically

The tool displays a clickable link in the console. If your terminal supports it, Ctrl+Click (or Cmd+Click on macOS) should open the link. Otherwise, copy and paste the URL into your browser manually.

### Script content is empty or unreadable

- Ensure you copied the correct `sessionKey` value from your browser
- Verify you're logged into the same NinjaOne instance
- Try logging out and back in to NinjaOne, then get a fresh session key

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE.txt](LICENSE.txt) file for details.

## Acknowledgments

Built for use with [NinjaOne](https://www.ninjaone.com/) RMM platform.