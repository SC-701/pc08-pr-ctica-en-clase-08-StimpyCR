using Abstracciones.Interfaces.Flujo;
using Flujo;
using Abstracciones.Interfaces.Reglas;
using Reglas;
using Abstracciones.Interfaces.Servicios;
using Servicios;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Autorizacion.Middleware;
using AccesoADatos;
using Abstracciones.Interfaces.AccesoADatos;
using AccesoADatos.Repositorios;

var builder = WebApplication.CreateBuilder(args);

// ★ Leer configuración JWT y registrar autenticación
var tokenConfig = builder.Configuration.GetSection("Token").Get<TokenConfiguracion>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = tokenConfig.Issuer,
            ValidAudience = tokenConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(tokenConfig.key))
        };
    });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Registro para configuración y HttpClient del servicio de tipo de cambio
builder.Services.AddScoped<IConfiguracion, Configuracion>();
builder.Services.AddHttpClient<ITipoCambioDolar, TipoCambioServicio>();

// Reglas y Flujo
builder.Services.AddScoped<IProductoReglas, ProductoReglas>();
builder.Services.AddScoped<IProductoFlujo, ProductoFlujo>();

// Acceso a datos
builder.Services.AddScoped<IProductoAD, ProductoAD>();
builder.Services.AddScoped<IRepositorioDapper, RepositorioDapper>();


// ★ Registrar servicios del paquete de Autorización
builder.Services.AddTransient<Autorizacion.Abstracciones.Flujo.IAutorizacionFlujo,
                               Autorizacion.Flujo.AutorizacionFlujo>();
builder.Services.AddTransient<Autorizacion.Abstracciones.DA.ISeguridadDA,
                               Autorizacion.DA.SeguridadDA>();
builder.Services.AddTransient<Autorizacion.Abstracciones.DA.IRepositorioDapper,
                               Autorizacion.DA.Repositorios.RepositorioDapper>();

var politicaAcceso = "Politica de acceso";


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: politicaAcceso,
                      policy =>
                      {
                          policy.WithOrigins("https://localhost", "https://localhost:50427", "http://localhost:5173")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(politicaAcceso);

app.AutorizacionClaims();
app.UseAuthorization();
app.MapControllers();
app.Run();
