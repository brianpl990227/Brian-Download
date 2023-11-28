using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using Figgle;
using XUCore.ShellProgressBar;



Console.WriteLine("-------------------------------------------------");
Console.WriteLine(FiggleFonts.Standard.Render("Brian - Download"));
Console.WriteLine("Descarga directorios enteros de https://visuales.uclv.cu");
Console.WriteLine("Github: https://github.com/brianpl990227/Brian-Download");
Console.WriteLine("-------------------------------------------------");


Console.Write("Pega la Url:");
string url = Console.ReadLine();
Console.WriteLine("Obteniendo información de directorios");
HttpClient client = new HttpClient();
try
{


    string html = await client.GetStringAsync(url);
    int totalFiles = 0;
    MatchCollection tdMatchesCount = Regex.Matches(html, "<td[^>]*>(.*?)</td>", RegexOptions.Singleline);
    foreach (Match tdMatch in tdMatchesCount)
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
                    totalFiles++;
                }
            }
        }
    }

    var currentFile = 0;
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
                    currentFile++;
                    Console.WriteLine("");
                    if (!Directory.Exists("Descargas"))
                    {
                        Directory.CreateDirectory("Descargas");
                    }

                    var name = Uri.UnescapeDataString(fileName);
                    var path = Path.Combine(AppContext.BaseDirectory, "Descargas", name);
                    if (File.Exists(path))
                    {
                        Console.WriteLine($"El fichero {name} ya existe");
                        continue;
                    }
                    try
                    {

                        using var response = await client.GetAsync(hrefValue, HttpCompletionOption.ResponseHeadersRead);

                        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                        using var stream = await response.Content.ReadAsStreamAsync();

                        var options = new ProgressBarOptions
                        {
                            DisplayTimeInRealTime = true,
                            ProgressBarOnBottom = true
                        };
                        using (var pbar = new ProgressBar((int)response.Content.Headers.ContentLength, $"Comenzando descarga de {name}", options))
                        {
                            var totalRead = 0L;
                            var buffer = new byte[8192];
                            var isMoreToRead = true;
                            var lastPercentage = 0.0;
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
                                    if (Math.Floor(percentage) > Math.Floor(lastPercentage))
                                    {
                                        pbar.Tick($"{name} | archivo {currentFile} de {totalFiles}");
                                        lastPercentage = percentage;
                                        var progress = pbar.AsProgress<double>();
                                        progress.Report(lastPercentage / 100);
                                    }

                                }
                            } while (isMoreToRead);


                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error al descargar el archivo: " + fileName);
                        Console.WriteLine(e.Message);
                        File.Delete(path);
                    }
                }
            }
        }
    }
    Console.WriteLine("\nDescarga completada :)");
    Console.ReadLine();
}
catch
{
    Console.WriteLine("Tienes la conexión muy lenta");
    Console.ReadLine();
}
