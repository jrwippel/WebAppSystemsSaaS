using System.Text;
using System.Text.Json;

namespace ClockTrack.Services
{
    public class MercadoPagoService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly string _accessToken;

        public MercadoPagoService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
            _accessToken = config["MercadoPago:AccessToken"] ?? "";
        }

        // Cria uma preferência de pagamento (checkout) e retorna a URL
        public async Task<MpPreferenceResult> CriarPreferenciaAsync(int tenantId, string plano, string emailPagador, string nomePagador)
        {
            var planos = ObterPlanos();
            if (!planos.TryGetValue(plano, out var info))
                throw new Exception("Plano inválido");

            var baseUrl = _config["App:BaseUrl"] ?? "https://clocktrack.azurewebsites.net";

            var body = new
            {
                items = new[]
                {
                    new
                    {
                        id = plano,
                        title = $"ClockTrack — Plano {info.Nome}",
                        description = info.Descricao,
                        quantity = 1,
                        currency_id = "BRL",
                        unit_price = info.Preco
                    }
                },
                payer = new { email = emailPagador, name = nomePagador },
                back_urls = new
                {
                    success = $"{baseUrl}/Assinatura/Sucesso?tenantId={tenantId}&plano={plano}",
                    failure = $"{baseUrl}/Assinatura/Falha",
                    pending = $"{baseUrl}/Assinatura/Pendente"
                },
                auto_return = "approved",
                external_reference = $"{tenantId}|{plano}",
                notification_url = $"{baseUrl}/Assinatura/Webhook",
                statement_descriptor = "CLOCKTRACK",
                expires = false
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mercadopago.com/checkout/preferences");
            request.Headers.Add("Authorization", $"Bearer {_accessToken}");
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro Mercado Pago: {json}");

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new MpPreferenceResult
            {
                Id = root.GetProperty("id").GetString() ?? "",
                InitPoint = root.GetProperty("init_point").GetString() ?? "",
                SandboxInitPoint = root.TryGetProperty("sandbox_init_point", out var s) ? s.GetString() ?? "" : ""
            };
        }

        // Busca um pagamento pelo ID para verificar status
        public async Task<MpPaymentInfo?> BuscarPagamentoAsync(string paymentId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.mercadopago.com/v1/payments/{paymentId}");
            request.Headers.Add("Authorization", $"Bearer {_accessToken}");

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new MpPaymentInfo
            {
                Id = root.GetProperty("id").GetInt64().ToString(),
                Status = root.GetProperty("status").GetString() ?? "",
                ExternalReference = root.TryGetProperty("external_reference", out var er) ? er.GetString() ?? "" : "",
                Plano = root.TryGetProperty("additional_info", out var ai) ? "" : ""
            };
        }

        public Dictionary<string, PlanoInfo> ObterPlanos() => new()
        {
            ["starter"]    = new PlanoInfo { Nome = "Starter",    Preco = 97m,  Descricao = "Ate 5 usuarios, Time Tracker ilimitado",    MaxUsers = 5,  DiasAssinatura = 30 },
            ["pro"]        = new PlanoInfo { Nome = "Pro",        Preco = 197m, Descricao = "Ate 15 usuarios, todos os recursos",         MaxUsers = 15, DiasAssinatura = 30 },
            ["enterprise"] = new PlanoInfo { Nome = "Enterprise", Preco = 397m, Descricao = "Usuarios ilimitados, suporte dedicado",      MaxUsers = 999, DiasAssinatura = 30 }
        };
    }

    public class MpPreferenceResult
    {
        public string Id { get; set; } = "";
        public string InitPoint { get; set; } = "";
        public string SandboxInitPoint { get; set; } = "";
    }

    public class MpPaymentInfo
    {
        public string Id { get; set; } = "";
        public string Status { get; set; } = "";
        public string ExternalReference { get; set; } = "";
        public string Plano { get; set; } = "";
    }

    public class PlanoInfo
    {
        public string Nome { get; set; } = "";
        public decimal Preco { get; set; }
        public string Descricao { get; set; } = "";
        public int MaxUsers { get; set; }
        public int DiasAssinatura { get; set; }
    }
}
