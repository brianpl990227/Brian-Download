using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using Figgle;
using HtmlAgilityPack;
using XUCore.ShellProgressBar;



Console.WriteLine("-------------------------------------------------");
Console.WriteLine(FiggleFonts.Standard.Render("Brian - Download"));
Console.WriteLine("Descarga directorios enteros de https://visuales.uclv.cu");
Console.WriteLine("Github: https://github.com/brianpl990227/brian-d");
Console.WriteLine("-------------------------------------------------");


Console.Write("Pega la Url:");
string url = Console.ReadLine();
HttpClient client = new HttpClient();
try
{


    string html = await client.GetStringAsync(url);

    // Extraer los td del html
    MatchCollection tdMatches = Regex.Matches(html, "<td[^>]*>(.*?)</td>", RegexOptions.Singleline);
    foreach (Match tdMatch in tdMatches)
    {
        // Extraer los href dentro de cada td
        MatchCollection hrefMatches = Regex.Matches(tdMatch.Value, "<a[^>]*href=\"(.*?)\"[^>]*>", RegexOptions.Singleline);

        foreach (Match hrefMatch in hrefMatches)
        {
            Uri baseUri = new Uri(url);
            string hrefValue = $"https://{baseUri.Host}{baseUri.AbsolutePath}{hrefMatch.Groups[1].Value}";

            // Comprobar si el href es una URL
            if (Uri.IsWellFormedUriString(hrefValue, UriKind.Absolute))
            {
                string fileName = Path.GetFileName(new Uri(hrefValue).AbsolutePath);
                if (!String.IsNullOrEmpty(fileName))
                {
                    try
                    {
                        if (!Directory.Exists("Descargas"))
                        {
                            Directory.CreateDirectory("Descargas");
                        }

                        using var response = await client.GetAsync(hrefValue, HttpCompletionOption.ResponseHeadersRead);
                        var name = Uri.UnescapeDataString(fileName);
                        var path = Path.Combine(AppContext.BaseDirectory, "Descargas", name);
                        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                        using var stream = await response.Content.ReadAsStreamAsync();
                        var test = Console.IsOutputRedirected;
                        using (var pbar = new ProgressBar((int)response.Content.Headers.ContentLength, $"Descargando {name}..."))
                        {

                            var totalRead = 0L;
                            var buffer = new byte[8192];
                            var isMoreToRead = true;

                            do
                            {
                                var read = await stream.ReadAsync(buffer, 0, buffer.Length);
                                if (read == 0)
                                {
                                    isMoreToRead = false;
                                }
                                else
                                {
                                    await fileStream.WriteAsync(buffer, 0, read);

                                    totalRead += read;
                                    var percentage = (double)totalRead / response.Content.Headers.ContentLength.Value * 100;
                                    pbar.Tick(read, $"Descargando {name}... {percentage:0.00}% completado");
                                }
                            } while (isMoreToRead);


                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error al descargar el archivo: " + fileName);
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
    }
}
catch
{
    Console.WriteLine("Tienes problemas de red");
    Console.ReadLine();
}
