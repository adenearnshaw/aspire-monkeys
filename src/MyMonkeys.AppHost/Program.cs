var builder = DistributedApplication.CreateBuilder(args);

var monkeysApi = builder.AddGolangApp("monkeys-service", "../MyMonkeys.Api")
    .WithHttpsEndpoint(port: 6001, env: "PORT")
    .WithEnvironment("USE_HTTPS", "true")
    .WithEnvironment("TLS_CERT_FILE", "certs/devcert.crt")
    .WithEnvironment("TLS_KEY_FILE", "certs/devcert.key");

var webBff = builder.AddProject<Projects.MyMonkeys_WebBff>("web-bff")
    .WithReference(monkeysApi);

var mobileBff = builder.AddProject<Projects.MyMonkeys_MobileBff>("mobile-bff")
    .WithReference(monkeysApi);

var webApp = builder.AddJavaScriptApp("web-app", "../MyMonkeys.Web")
    .WithRunScript("start")
    .WithHttpsEndpoint(port: 6022, env: "PORT")
    .WithEnvironment("WEB_BFF_URL", webBff.GetEndpoint("https"))
    .WithReference(webBff);

var mobileApp = builder.AddMauiProject("mobile-app", "../MyMonkeys.Mobile/MyMonkeys.Mobile.csproj");

var macApp = mobileApp.AddMacCatalystDevice()
    .WithReference(mobileBff);

builder.Build().Run();
