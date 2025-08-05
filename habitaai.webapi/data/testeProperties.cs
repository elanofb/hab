using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.Encodings.Web;
using habitaai.webapi.Utils;

//namespace habitaai.webapi.Utils
//{
//    public static class Helper
//    {
//        public static string Hello()
//        {
//            return "Oláaaarr! Chamado de dentro do código dinâmico.";
//        }


//    }
//}

public class Entrada
{
    public string Nome { get; set; }
}

//public class DynamicHandler
//{
//    //public static async Task hello_input(HttpContext context)
//    internal void ExecuteAsync(HttpContext context)
//    {
//        context.Request.EnableBuffering();
//        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
//        //var body = await reader.ReadToEndAsync();
//        var body = reader.ReadToEnd();
//        context.Request.Body.Position = 0;

//        var entrada = JsonSerializer.Deserialize<Entrada>(body);

//        var mensagem = "elano ";
//        var nome = entrada?.Nome ?? "barreto";

//        context.Response.ContentType = "application/json; charset=utf-8";
//        var resultado = new { Mensagem = $"{mensagem}{nome}! Requisição dinâmica com POST recebida." };

//        var options = new JsonSerializerOptions
//        {
//            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
//            WriteIndented = true
//        };

//        var json = JsonSerializer.Serialize(resultado, options);
//        //await context.Response.WriteAsync(json, Encoding.UTF8);
//        context.Response.WriteAsync(json, Encoding.UTF8);
//    }
//}

public class DynamicHandler
{
    //public static void ExecuteAsync()//HttpContext context)
    internal void ExecuteAsync()
    {

        //await Helper.GetHttp(context);

        //context.Response.ContentType = "application/json; charset=utf-8";

        var connectionString = "Server=DESKTOP-O3LGOT5;Database=HabitaAiDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;";
        var results = new List<object>();

        using (var connection = new SqlConnection(connectionString))
        {
            //await connection.OpenAsync();
            connection.Open();

            //var command = new SqlCommand("SELECT Id, Title, Price FROM Property", connection);
            var command = new SqlCommand("SELECT Id, Title, Price FROM Properties", connection);
            //using (var reader = await command.ExecuteReaderAsync())
            using (var reader = command.ExecuteReader())
            {
                //while (await reader.ReadAsync())
                while (reader.Read())
                {
                    results.Add(new
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Price = reader.GetDecimal(2)
                    });
                }
            }
        }

        var json = JsonSerializer.Serialize(results);
        //await json;
        //await context.Response.WriteAsync(json, Encoding.UTF8);
    }

}