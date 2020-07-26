# NFig.AspNetCore

![Build status](https://github.com/NFig/NFig.AspNetCore/workflows/Build,%20Test%20&%20Package/badge.svg)

Provides ASP.NET Core support for NFig by integrating into the options framework.

#### Package Status

MyGet Pre-release feed: https://www.myget.org/gallery/stackoverflow

| Package | NuGet Stable | NuGet Pre-release | Downloads | MyGet |
| ------- | ------------ | ----------------- | --------- | ----- |
| [NFig.AspNetCore](https://www.nuget.org/packages/NFig.AspNetCore/) | [![NFig.AspNetCore](https://img.shields.io/nuget/v/NFig.AspNetCore.svg)](https://www.nuget.org/packages/NFig.AspNetCore/) | [![NFig.AspNetCore](https://img.shields.io/nuget/vpre/NFig.AspNetCore.svg)](https://www.nuget.org/packages/NFig.AspNetCore/) | [![NFig.AspNetCore](https://img.shields.io/nuget/dt/NFig.AspNetCore.svg)](https://www.nuget.org/packages/NFig.AspNetCore/) | [![NFig.AspNetCore MyGet](https://img.shields.io/myget/stackoverflow/vpre/NFig.AspNetCore.svg)](https://www.myget.org/feed/stackoverflow/package/nuget/NFig.AspNetCore) |

## Getting Started

Integrating NFig into your ASP.NET Core application is easy! 

### Installation

Install the `[NFig.AspNetCore](https://www.nuget.org/packages/NFig.AspNetCore/)` NuGet package using:

```bat
dotnet add <project> package NFig.AspNetCore
```

**If setting up a web project it's a good idea to check out the [sample project](samples/NFig.AspNetCore.Sample).**

### Configuration

NFig can be configured by using the code below. You can see we have defined a `Settings` class containing the settings our application needs, a `Tier` enum representing the different tiers our application can run in and a `DataCenter` enum representing the data centers that our application runs in.

We add a call to `AddNFig` in our `ConfigureServices` method specifying the types above so NFig knows how to manage our settings. Under the hood this tells the DI container how to resolve an `IOptions<Settings>` and also `IOptions<T>` where `T` is any setting group specified within our settings (e.g. `FeatureFlagSettings`).

Next we call `UseNFig` in our `Configure` method. This is passed a builder that can be used to configure NFig's backing store. Right now NFig only has Redis as a backing store so we simply configure Redis using a connection string from our configuration file. Our configuration file also tells us what tier and data center our application is running in, so we pass those to NFig as well.

That's it!

**appsettings.json**

```json
{
    "ApplicationName": "NFig Example",
    "Tier": "Local",
    "DataCenter": "Local",
    "ConnectionStrings": {
        "Redis": "localhost:6379,defaultDatabase=0"
    }
}
```

**Startup.cs**

```c#
public class Settings
{
    [SettingsGroup]
    public FeatureFlagSettings FeatureFlags { get; private set; }

    public class FeatureFlagSettings
    {
        [Description("Enables the [Whizzbang](https://trello.com/.../) feature.")]
        [Setting(false)]
        [TieredDefaultValue(Tier.Local, true)]
        [TieredDefaultValue(Tier.Dev, true)]
        public bool EnableWhizzbang { get; private set; }
    }
}

public enum Tier
{
    Local,
    Dev,
    Test,
    Prod,
}

public enum DataCenter
{
    Local,
    Timbuktu,    
}

public class AppSettings
{
    public string ApplicationName { get; set; }
    public Tier Tier { get; set; }
    public DataCenter { get; set; }
}

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // add services needed by NFig
        // under the hood this configures the DI container
        // to resolve NFig settings as IOptions implementations
        services.AddNFig<Settings, Tier, DataCenter>();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        // configure NFig's backing store
        app
            .UseNFig<Settings, Tier, DataCenter>(
                (cfg, builder) =>
                {
                    var settings = cfg.Get<AppSettings>();
                    var connectionString = cfg.GetConnectionString("Redis");

                    // connects NFig to Redis
                    builder.UseRedis(settings.ApplicationName, settings.Tier, settings.DataCenter, connectionString);
                }
            );
    }
}
```

### Exposing NFig UI

NFig.AspNetCore contains middleware that can render NFig.UI. It can be called from within MVC or by a route handler in Kestrel. This way you can lock down the route using your current security models.

**Using MVC**

```c#
public class SettingsController
{
    // restrict this route to users in the Admins role
    [Authorize(Roles = "Admins")]
    [Route("settings/{*pathInfo}")]
    public Task Settings() => NFigMiddleware<Settings, Tier, DataCenter>.HandleRequestAsync(HttpContext);
}
```

**Using Kestrel**

```c#
public class Startup
{
    // ... other startup bits here

    public void Configure(IAppBuilder app, IHostingEnvironment env)
    {
        app.MapWhen(
            ctx => ctx.Request.Path.StartsWithSegments("/settings"),
            appBuilder => appBuilder.Run(NFigMiddleware<Settings, Tier, DataCenter>.HandleRequestAsync)
        );
    }
}
```

### Using Our Settings

To use our settings all we have to do is inject them wherever we need to use them.

**In an MVC Controller**

```c#
public class HomeController : Controller
{
    private readonly FeatureFlagSettings _featureFlags;

    public HomeController(IOptions<FeatureFlagSettings> featureFlags)
    {
        _featureFlags = featureFlags.Value;
    }

    public IActionResult Home()
    {
        if (_featureFlags.EnableWhizzbang)
        {
            return Content("Whizzbang enabled!");
        }

        return Content("Whizzbang disabled");
    }
}
```

**In an MVC View**

```html
@model MyModel
@inject IOptions<FeatureFlagSettings> featureFlags

@if (featureFlags.Value.EnableWhizzbangFeature)
{
    <strong>Whizzbang enabled</strong>
}
else
{
    <strong>Whizzbang disabled</strong>
}

```

### Futures
 - support NFig 3.x
 - some tests would be nice
