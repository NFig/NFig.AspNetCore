using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NFig;
using NFig.Redis;
using NFig.UI;
using StackExchange.Redis;

namespace NFig.AspNetCore
{

    public delegate void SettingsUpdateDelegate<TSettings, TTier, TDataCenter>(Exception ex, TSettings settings)
        where TSettings : class, INFigSettings<TTier, TDataCenter>, new()
        where TTier : struct
        where TDataCenter : struct;
}
