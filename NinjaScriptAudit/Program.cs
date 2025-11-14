using NinjaScriptAudit;
using System.Collections.Specialized;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using System.Text.Json;

Option<string> instanceOption = new("--instance", "-i")
{
    Description = "The NinjaOne instance to audit.",
    Required = true
};

Option<string> clientIdOption = new("--client-id", "-c")
{
    Description = "The client ID for authentication.",
    Required = true
};

Option<string> clientSecretOption = new("--client-secret", "-s")
{
    Description = "The client secret for authentication.",
    Required = true
};

Option<string> scopeOption = new("--scope", "-o")
{
    Description = "The scope for authentication.",
    Required = true
};

Option<string> redirectUriOption = new("--redirect-uri", "-r")
{
    Description = "The redirect URI for the local HTTP listener.",
    Required = false,
    DefaultValueFactory = (_) => "http://localhost:8080/"
};

Option<string> searchRegexOption = new("--search-regex", "-x")
{
    Description = "The regex pattern to search for within scripts.",
    Required = true
};

RootCommand rootCommand = new("NinjaScript Audit Tool");
rootCommand.Options.Add(instanceOption);
rootCommand.Options.Add(clientIdOption);
rootCommand.Options.Add(clientSecretOption);
rootCommand.Options.Add(scopeOption);
rootCommand.Options.Add(redirectUriOption);
rootCommand.Options.Add(searchRegexOption);

ParseResult parseResult = rootCommand.Parse(args);
if (parseResult.Errors.Count > 0)
{
    foreach (ParseError error in parseResult.Errors)
    {
        Console.Error.WriteLine(error.Message);
    }
    return 1;
}

string instance = parseResult.GetValue(instanceOption) ?? throw new InvalidOperationException("Instance is required.");
string clientId = parseResult.GetValue(clientIdOption) ?? throw new InvalidOperationException("Client ID is required.");
string clientSecret = parseResult.GetValue(clientSecretOption) ?? throw new InvalidOperationException("Client secret is required.");
string scope = parseResult.GetValue(scopeOption) ?? throw new InvalidOperationException("Scope is required.");
string redirectUri = parseResult.GetValue(redirectUriOption) ?? throw new InvalidOperationException("Redirect URI is required.");
string searchRegex = parseResult.GetValue(searchRegexOption) ?? throw new InvalidOperationException("Search regex is required.");

if (redirectUri[^1] != '/')
{
    redirectUri += '/';
}
HttpListener listener = new();
listener.Prefixes.Add(redirectUri);
listener.Start();
instance = instance.Replace("https://", "").Replace("http://", "").TrimEnd('/');
// https://provaltech.rmmservice.com/ws/oauth/authorize?response_type=code&client_id=jF2iBFGS-4PcJSXI2BZOJPWNQVk&client_secret=9AAneb4DfkYDDsXVO0-1V1Via1mdgFSYqJ6uZfR2OmRfSYXVdfAy7Q&redirect_uri=http://localhost:8080&scope=offline_access%20monitoring%20management%20control
UriBuilder uriBuilder = new($"https://{instance}/ws/oauth/authorize");
uriBuilder.Query = $"response_type=code&client_id={WebUtility.UrlEncode(clientId)}&client_secret={WebUtility.UrlEncode(clientSecret)}&redirect_uri={WebUtility.UrlEncode(redirectUri)}&scope={WebUtility.UrlEncode(scope)}";
static string TerminalURL(string caption, string url) => $"\u001B]8;;{url}\a{caption}\u001B]8;;\a";

Uri authUri = uriBuilder.Uri;
Console.WriteLine("Go to the following URL to authorize the application:");
Console.WriteLine(TerminalURL("NinjaRMM Link", authUri.ToString()));
string accessCode = string.Empty;
while (listener.IsListening)
{
    HttpListenerContext context = listener.GetContext();
    HttpListenerRequest request = context.Request;
    HttpListenerResponse response = context.Response;
    NameValueCollection query = request.QueryString;
    string? code = query.Get("code");
    if (!string.IsNullOrEmpty(code))
    {
        accessCode = code;
        string responseString = """
    <!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <title>Authorization Complete</title>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; margin: 0; background: #0f172a; color: #e2e8f0; }
        .card { max-width: 540px; margin: 10% auto; background: #111827; padding: 32px; border-radius: 18px; box-shadow: 0 20px 40px rgba(15, 23, 42, 0.6); }
        h1 { margin-top: 0; font-size: 1.8rem; }
        p { line-height: 1.5; }
        ol { padding-left: 1.2rem; }
        ol li { margin: 0.35rem 0; }
        .pill { display: inline-block; padding: 6px 12px; border-radius: 999px; background: #1d4ed8; color: #fff; font-size: 0.85rem; letter-spacing: 0.04em; text-transform: uppercase; }
    </style>
</head>
<body>
    <div class="card">
        <span class="pill">Step Complete</span>
        <h1>Authorization Successful</h1>
        <p>Before returning to the console, copy your <strong>sessionKey</strong> cookie so the audit can access your scripts.</p>
        <ol>
            <li>Open your NinjaOne tab and press <strong>F12</strong> to launch DevTools.</li>
            <li>Navigate to <em>Application</em> (Chrome/Edge) or <em>Storage</em> (Firefox).</li>
            <li>Expand <strong>Cookies</strong>, select your NinjaOne URL, then copy the <code>sessionKey</code> value.</li>
        </ol>
        <p>All set—you can now close this window.</p>
    </div>
</body>
</html>
""";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        using Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        break;
    }
    else
    {
        string responseString = "<html><body><h1>Authorization Failed</h1><p>No authorization code received.</p></body></html>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        using Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
    }
}
listener.Stop();
HttpClient httpClient = new() {
    BaseAddress = new Uri($"https://{instance}/ws/oauth/token")
};
httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
// Set up the body as application/x-www-form-urlencoded
FormUrlEncodedContent body = new(
    [
        new KeyValuePair<string, string>("grant_type", "authorization_code"),
        new KeyValuePair<string, string>("code", accessCode),
        new KeyValuePair<string, string>("redirect_uri", redirectUri),
        new KeyValuePair<string, string>("client_id", clientId),
        new KeyValuePair<string, string>("client_secret", clientSecret)
    ]
);
HttpResponseMessage responseMessage = await httpClient.PostAsync(httpClient.BaseAddress, body);
string responseContent = await responseMessage.Content.ReadAsStringAsync();
OAuthTokenResponse? tokenResponse = JsonSerializer.Deserialize(responseContent, SourceGenerationContext.Default.OAuthTokenResponse);
if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
{
    Console.Error.WriteLine("Failed to obtain access token.");
    return 1;
}
httpClient = new();
httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
httpClient.BaseAddress = new Uri($"https://{instance}/v2/automation/scripts");
HttpResponseMessage scriptsResponse = await httpClient.GetAsync(httpClient.BaseAddress);
string scriptsContent = await scriptsResponse.Content.ReadAsStringAsync();
List<ScriptMetadata>? scripts = JsonSerializer.Deserialize(scriptsContent, SourceGenerationContext.Default.ListScriptMetadata);
if (scripts == null)
{
    Console.Error.WriteLine("Failed to retrieve scripts.");
    return 1;
}
Console.WriteLine("Enter your sessionKey cookie value:");
string sessionKeyValue = Console.ReadLine() ?? string.Empty;
foreach (ScriptMetadata script in scripts)
{
    string scriptUrl = $"https://{instance}/swb/s7/scripting/scripts/{script.Id}";
    HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, scriptUrl);
    httpRequestMessage.Headers.Add("Cookie", $"sessionKey={sessionKeyValue}");
    HttpResponseMessage scriptPageResponse = await httpClient.SendAsync(httpRequestMessage);
    string scriptPageContent = await scriptPageResponse.Content.ReadAsStringAsync();
    Script scriptData = JsonSerializer.Deserialize(scriptPageContent, SourceGenerationContext.Default.Script) ?? throw new InvalidOperationException("Failed to deserialize script.");
    string scriptContent = Convert.FromBase64String(scriptData.Code ?? string.Empty) is byte[] scriptBytes
        ? System.Text.Encoding.UTF8.GetString(scriptBytes)
        : string.Empty;
    if (System.Text.RegularExpressions.Regex.IsMatch(scriptContent, searchRegex))
    {
        Console.WriteLine($"Match found in script ID {script.Id}, Name: {script.Name}");
    }
}
return 0;