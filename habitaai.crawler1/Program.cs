using System.Net;
using System.Net.Http.Headers;
using HtmlAgilityPack;
using Microsoft.Playwright;
using System.Text;
using System.Globalization;

class Program
{
    static async Task Main(string[] args)
    {
        var url = "https://www.zapimoveis.com.br/imovel/aluguel-casa-6-quartos-mobiliado-cumbuco-caucaia-ce-250m2-id-2557824056/?source=ranking%2Crp";

        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/91.0.4472.124 Safari/537.36",
            Locale = "pt-BR"
        });

        var page = await context.NewPageAsync();
        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await page.ScreenshotAsync(new PageScreenshotOptions { Path = "screenshot.png", FullPage = true });
        Console.WriteLine("📷 Screenshot salva como 'screenshot.png'");

        // ✅ Salvar o HTML completo da página
        var htmlContent = await page.ContentAsync();
        await File.WriteAllTextAsync("pagina_renderizada_zap.html", htmlContent, Encoding.UTF8);
        Console.WriteLine("📝 HTML salvo como 'pagina_renderizada_zap.html'");
        //var htmlDump = await page.ContentAsync();
        //await File.WriteAllTextAsync("pagina_renderizada_zap.html", htmlDump);
        //Console.WriteLine("📝 HTML renderizado salvo como 'pagina_renderizada_zap.html'");

        //-------------------------------------
        // Carrega o HTML renderizado
        var html = await File.ReadAllTextAsync("pagina_renderizada_zap.html", Encoding.UTF8);

        // Carrega no HtmlDocument
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // 🔍 Extrair Título
        var tituloNode = doc.DocumentNode.SelectSingleNode("//h1");
        var titulo = tituloNode?.InnerText.Trim() ?? "(não encontrado)";

        // 🔍 Extrair Preço
        var precoNode = doc.DocumentNode.SelectSingleNode("//strong[contains(@data-testid,'price-value')]");
        var preco = precoNode?.InnerText.Trim() ?? "(não encontrado)";

        // 🔍 Extrair Endereço
        var enderecoNode = doc.DocumentNode.SelectSingleNode("//p[contains(@data-testid,'address')]");
        var endereco = enderecoNode?.InnerText.Trim() ?? "(não encontrado)";

        // Exibir no console
        Console.WriteLine($"📌 Título: {titulo}");
        Console.WriteLine($"💰 Preço: {preco}");
        Console.WriteLine($"📍 Endereço: {endereco}");

        // Salvar CSV
        var csv = new StringBuilder();
        csv.AppendLine("Titulo,Preco,Endereco");
        csv.AppendLine($"\"{titulo}\",\"{preco}\",\"{endereco}\"");

        await File.WriteAllTextAsync("dados_extraidos.csv", csv.ToString(), Encoding.UTF8);
        Console.WriteLine("✅ Dados salvos em 'dados_extraidos.csv'");


        //-------------------------------------
        //✅ Resultado
        //// ✅ Extrair dados com seletores atualizados (inspecionados em 2025)
        //var titulo = await page.TextContentAsync("h1");
        ////var preco = await page.TextContentAsync("strong[data-testid='price-value']");
        //await page.Locator("strong[data-testid='price-value']").WaitForAsync(new LocatorWaitForOptions { Timeout = 60000 });
        //var preco = await page.Locator("strong[data-testid='price-value']").InnerTextAsync();
        //var endereco = await page.TextContentAsync("p[data-testid='address']");

        //titulo = titulo?.Trim() ?? "(não encontrado)";
        //preco = preco?.Trim() ?? "(não encontrado)";
        //endereco = endereco?.Trim() ?? "(não encontrado)";

        //Console.WriteLine($"\n📌 Título: {titulo}");
        //Console.WriteLine($"💰 Preço: {preco}");
        //Console.WriteLine($"📍 Endereço: {endereco}");

        // ✅ Gravar em CSV
        var csv2 = new StringBuilder();
        csv2.AppendLine("Titulo,Preco,Endereco");
        csv2.AppendLine($"\"{titulo}\",\"{preco}\",\"{endereco}\"");

        await File.WriteAllTextAsync("dados_imovel.csv", csv2.ToString(), Encoding.UTF8);
        Console.WriteLine("✅ Dados salvos em 'dados_imovel.csv'");

        await browser.CloseAsync();
    }
    //static async Task Main(string[] args)
    //{
    //    var url = "https://www.zapimoveis.com.br/imovel/aluguel-casa-6-quartos-mobiliado-cumbuco-caucaia-ce-250m2-id-2557824056/?source=ranking%2Crp";

    //    using var playwright = await Playwright.CreateAsync();
    //    var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
    //    {
    //        Headless = true // altere para false se quiser ver o navegador em ação
    //    });

    //    var context = await browser.NewContextAsync(new BrowserNewContextOptions
    //    {
    //        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/91.0.4472.124 Safari/537.36",
    //        Locale = "pt-BR"
    //    });

    //    var page = await context.NewPageAsync();
    //    await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

    //    // ✅ Salvar HTML completo
    //    var htmlContent = await page.ContentAsync();
    //    File.WriteAllText("pagina_renderizada_zap.html", htmlContent);
    //    Console.WriteLine("✅ HTML completo salvo em 'pagina_renderizada_zap.html'.");

    //    // ✅ Extrair dados (ajuste os seletores conforme o site)
    //    var titulo = await page.TextContentAsync("h1");
    //    var preco = await page.TextContentAsync("p[class*=Price]"); // pode variar
    //    var endereco = await page.TextContentAsync("p[class*=Address]");

    //    Console.WriteLine($"\n📌 Título: {titulo?.Trim()}");
    //    Console.WriteLine($"💰 Preço: {preco?.Trim()}");
    //    Console.WriteLine($"📍 Endereço: {endereco?.Trim()}");

    //    await browser.CloseAsync();
    //}
}
