//using habitaai.webapi.Dynamic;
//using habitaai.webapi.domain;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
//);
//builder.Services.AddCors(opt =>
//{
//    opt.AddPolicy("ReactPolicy", policy =>
//    {
//        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
//    });
//});

//builder.Services.AddSingleton<IEndpointRouteBuilder>(sp =>
//{
//    var app = sp.GetRequiredService<IApplicationBuilder>();
//    return app as IEndpointRouteBuilder;
//});

//var app = builder.Build();

//app.UseCors("ReactPolicy");
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseAuthorization();

//app.UseSwagger();
//app.UseSwaggerUI();

//app.MapControllers();

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    var endpoints = db.DynamicEndpoints.Where(e => e.Enabled).ToList();

//    foreach (var ep in endpoints)
//    {
//        try
//        {
//            //var handler = DynamicCompiler.Compile(ep.SourceCode, ep.MethodName);
//            var handler = DynamicCompiler.Compile(ep.SourceCodeFull, ep.MethodName);
//            DynamicControllerHelper.Register(ep.Route, handler);
//        }
//        catch (Exception ex)
//        {
//            Logger.Log(ep.Route, ep, null, ex); // Opcional: registrar erro
//        }
//    }
//}

////Adiciona endpoints dinâmicos registrados
//foreach (var route in DynamicControllerHelper.RegisteredHandlers)
//{
//    app.Map(route.Key, route.Value);
//}

//// Middleware para interceptar e executar rotas dinâmicas
//app.Map("/{**catchall}", async context =>
//{
//    var path = context.Request.Path.ToString().Trim().TrimStart('/').ToLowerInvariant();

//    if (DynamicControllerHelper.RegisteredHandlers.TryGetValue(path, out var handler))
//    {
//        try
//        {
//            await handler(context);
//        }
//        catch (Exception ex)
//        {
//            context.Response.StatusCode = 500;
//            await context.Response.WriteAsync($"Erro no endpoint dinâmico: {ex.Message}");
//        }
//    }
//    else
//    {
//        context.Response.StatusCode = 404;
//        await context.Response.WriteAsync("Rota dinâmica não encontrada.");
//    }
//});

//app.Run();


////////////////////////
///
/// 
///
using habitaai.webapi.Dynamic;
using habitaai.webapi.domain;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("ReactPolicy", policy =>
    {
        policy.AllowAnyOrigin().
        AllowAnyHeader().
        AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IEndpointRouteBuilder>(sp =>
{
    var app = sp.GetRequiredService<IApplicationBuilder>();
    return app as IEndpointRouteBuilder;
});

var app = builder.Build();
//app.UseRouting();
app.UseCors("ReactPolicy");
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var endpoints = db.DynamicEndpoints.Where(e => e.Enabled).ToList();

    foreach (var ep in endpoints)
    {
        try
        {
            //var handler = DynamicCompiler.Compile(ep.SourceCode, ep.MethodName);
            var handler = DynamicCompiler.Compile(ep.SourceCodeFull, ep.MethodName);
            DynamicControllerHelper.Register(ep.Route, handler);
        }
        catch (Exception ex)
        {
            Logger.Log(ep.Route, ep, null, ex); // Opcional: registrar erro
        }
    }
}
////Adiciona endpoints dinâmicos registrados
//foreach (var route in DynamicControllerHelper.RegisteredHandlers)
//{
//    app.Map(route.Key, route.Value);
//}

// Middleware para interceptar e executar rotas dinâmicas
app.Map("/{**catchall}", async context =>
{
    var path = context.Request.Path.ToString().Trim().TrimStart('/').ToLowerInvariant();

    // Protege rotas conhecidas (como Swagger) para não serem interceptadas
    var rotasProtegidas = new[]
    {
        "swagger", "favicon.ico", "index.html", "css", "js", "lib"
    };

    if (rotasProtegidas.Any(p => path.StartsWith(p)))
    {
        return;
    }

    // Tratamento de preflight OPTIONS
    if (context.Request.Method == HttpMethods.Options)
    {
        context.Response.StatusCode = 204;
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        return;
    }

    if (DynamicControllerHelper.RegisteredHandlers.TryGetValue(path, out var handler))
    {
        try
        {
            await handler(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"Erro no endpoint dinâmico: {ex.Message}");
        }
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Rota dinâmica não encontrada.");
    }
});

app.Run();
