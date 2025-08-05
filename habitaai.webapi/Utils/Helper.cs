using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.Encodings.Web;
using System.Net;

namespace habitaai.webapi.Utils
{
    public static class Helper
    {
        public static string Hello()
        {
            return "Oláaaarr! Chamado de dentro do código dinâmico.";
        }

        public static async Task GetHttp(HttpContext context)
        {
            var client = new HttpClient();
            var apiUrl = "https://localhost:7071/api/getproperties";
            var response = await client.GetAsync(apiUrl);
            var body = await response.Content.ReadAsStringAsync();
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(body, Encoding.UTF8);
        }

        public static string GetAll()
        {
            var connectionString = "Server=DESKTOP-O3LGOT5;Database=HabitaAiDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;";
            object result = null;
            var results = new List<object>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Title, Price FROM Properties", connection);
                using (var reader = command.ExecuteReader())
                {
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
            result = results;

            //context.Response.WriteAsync(json, Encoding.UTF8);
            return json;

        }

        public static string RetornaSourceFull(string methodName, string sourceCode, bool isCrud)
        {
            string result = @"
                using System;
                using System.Net.Http;
                using System.Text.Encodings.Web;
                using System.Collections.Generic;
                using Microsoft.Data.SqlClient;
                using System.Text;
                using System.Text.Json;
                using System.Threading.Tasks;
                using Microsoft.AspNetCore.Http;
                using habitaai.webapi.Utils;    
                using System.IO;

                public class Entrada_" + methodName + @"
                {
                    public string Nome { get; set; }
                }

                public class DynamicHandler_" + methodName + @"
                {
                    public static async Task " + methodName + @"(HttpContext context)
                    {";

            if (isCrud)
            {
                result += @"     
                        var metodo = context.Request.Method;

                        if (metodo == ""GET"")
                        {
                            await Helper.GetHttp(context);
                            
                        }
                        else if (metodo == ""POST"")
                        {
                            context.Request.EnableBuffering();
                            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                            var body = await reader.ReadToEndAsync();
                            context.Request.Body.Position = 0;                            
                            var entrada = JsonSerializer.Deserialize<JsonElement>(body);

                            var nome = entrada.GetProperty(""nome"").GetString();
                            var result = new { Mensagem = $""Olá {nome}, método POST recebido!"" };
                            var json = JsonSerializer.Serialize(result);
                            await context.Response.WriteAsync(json);
                        }
                        else
                        {
                            context.Response.StatusCode = 405;
                            await context.Response.WriteAsync(""Método não permitido."");
                        }";
            }
            else
            {
                result += sourceCode;
            }

            result += @"
                    }
                }
            ";
            return result;
        }
    }
}
