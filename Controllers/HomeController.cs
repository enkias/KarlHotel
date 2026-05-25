using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using KarlHotel.Models;

namespace KarlHotel.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _cfg;

    public HomeController(IHttpClientFactory http, IConfiguration cfg)
    {
        _http = http;
        _cfg = cfg;
    }

    public IActionResult Index() => View();
    public IActionResult Restaurace() => View();
    public IActionResult Vylety() => View();
    public IActionResult Galerie() => View();

    [HttpPost("api/chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.Message)) return BadRequest();

        const string system = """
            Jsi hotelový asistent Hotelu Karl na Špičáku (Železná Ruda, Šumava). Odpovídáš česky, stručně a přátelsky.

            Základní informace:
            - Hotel Karl***, Špičák 144, 340 04 Železná Ruda
            - Tel: +420 376 397 116, Recepce: +420 739 015 629
            - E-mail: hotelkarl@seznam.cz
            - Rodinný hotel postaven 2001, nadmořská výška ~1000 m

            Ubytování (14 pokojů):
            - 1× apartmá se 2 ložnicemi (6 lůžek) – obývací pokoj, koupelna, balkon, TV/SAT, lednička
            - 9× apartmá s 1 ložnicí – obývací pokoj, koupelna, některé s balkonem, lednička
            - 4× dvoulůžkový pokoj – ložnice + koupelna, lednička
            - Snídaně formou švédských stolů, Wi-Fi zdarma v recepci
            - Parkoviště zdarma, garáž 200 Kč/den, domácí zvířata 100 Kč/den

            Ceny 2026 (lůžko + snídaně, od 2 nocí):
            - Dospělí: 1 190 Kč/os/noc (apartmá), 1 390 Kč/os/noc (jednolůžkový)
            - Děti 3–12 let: 690 Kč/os/noc
            - Silvestr (28.12–2.1): 1 500 / 1 700 Kč + silvestrovská večeře (buffet) 890 Kč
            - Příplatek za 1 noc: +100 %
            - Večeře: 390 Kč dospělí, 290 Kč děti

            Restaurace: čtvrtek–neděle 11:00–19:30

            Vybavení hotelu:
            - Přímo u sjezdovky (ski-in/ski-out) – lyžařský areál Špičák u dveří
            - Parkování zdarma v areálu, garáž 200 Kč/den
            - Domácí zvířata povolena 100 Kč/den
            - Vlastní koupelna a toaleta, sprcha
            - Lednička, satelitní TV na každém pokoji
            - Terasa
            - Restaurace, snack bar, obědové balíčky
            - Půjčení kola, dětské hřiště
            - Dětská jídla na vyžádání, speciální dietní menu
            - Individuální check-in/check-out
            - Wi-Fi v recepci (na pokojích není internet)
            - Prostor pro kuřáky, kamerový systém
            - Personál hovoří: česky, německy, anglicky, španělsky, slovensky

            Aktivity v okolí:
            - Černé jezero 3,5 km, Čertovo jezero 2,5 km (okruh 9 km)
            - Vodopád Bílá strž 6 km
            - Vrchol Pancíř a Špičák do 3 km, lanovka
            - Bike areál Špičák – 6 bikerských tras světové úrovně, Downhill World Cup
            - Zimně: lyžařský areál Špičák přímo u hotelu

            Rezervace: přímou rezervací získají hosté nejlepší cenu a osobní přístup.
            Při dotazu na rezervaci nasměruj na formulář na webu nebo na telefon.
            """;

        var body = new
        {
            model = "claude-haiku-4-5-20251001",
            max_tokens = 400,
            system,
            messages = new[] { new { role = "user", content = req.Message } }
        };

        var client = _http.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", _cfg["ClaudeApiKey"]);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var resp = await client.PostAsync(
            "https://api.anthropic.com/v1/messages",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        if (!resp.IsSuccessStatusCode) return StatusCode(500);

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var text = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString();
        return Json(new { reply = text });
    }

    [HttpPost("rezervace")]
    public IActionResult Rezervace([FromForm] RezervaceRequest req)
    {
        TempData["Success"] = "Děkujeme! Vaši poptávku jsme přijali. Ozveme se do 24 hodin.";
        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}

public record ChatRequest(string Message);
public record RezervaceRequest(string Jmeno, string Email, string Telefon, string Od, string Do, string TypPokoje, int Osoby, string? Poznamka);
