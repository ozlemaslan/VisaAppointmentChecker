using Newtonsoft.Json;
using RabbitMQ.Client;
using VisaChecker.Models;
using VisaChecker.Services;

namespace VisaChecker
{
    public class VisaAppointmentCheckerWorker(IConnectionFactory connectionFactory) : BackgroundService
    {
        private readonly RabbitMqService _rabbitMqService = new RabbitMqService(connectionFactory);
        private readonly string _apiUrl = "https://api.schengenvisaappointments.com/api/visa-list/?format=json";
        private readonly int _intervalInMinutes = 1; // Kontrol sıklığı (dakika)

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Visa Appointment Checker başlatıldı...");
            Console.WriteLine($"Kontrol sıklığı: {_intervalInMinutes} dakika");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine("API'den veri çekiliyor...");
                    var appointments = await FetchVisaAppointments(_apiUrl);

                    Console.WriteLine("Veriler filtreleniyor...");

                    //fransa için vize randevusu aranıyor.
                    var filteredAppointments = appointments.Where(a =>
                        a.SourceCountry.Contains("Turkiye", StringComparison.OrdinalIgnoreCase) &&
                        a.MissionCountry.Contains("FRANCE", StringComparison.OrdinalIgnoreCase) &&
                        a.CenterName.Contains("Ankara", StringComparison.OrdinalIgnoreCase) &&
                        a.VisaSubCategory.Contains("Tourism", StringComparison.OrdinalIgnoreCase) &&
                        a.AppointmentDate != null).ToList();


                    if (filteredAppointments.Any())
                    {
                        Console.WriteLine($"Uygun randevular bulundu: {filteredAppointments.Count} adet.");
                        _rabbitMqService.SendtoRabbitMq(filteredAppointments);

                        Console.WriteLine("RabbitMQ üzerinden mesajlar dinleniyor ve Telegram'a gönderiliyor...");
                        _rabbitMqService.ListentoRabbitMqAndSendtoTelegram();
                    }
                    else
                    {
                        Console.WriteLine("Uygun randevu bulunamadı.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata: {ex.Message}");
                }

                Console.WriteLine($"Bir sonraki kontrol {_intervalInMinutes} dakika sonra...");
                await Task.Delay(_intervalInMinutes * 60 * 1000, stoppingToken); // Bekleme
            }
        }

        private static async Task<List<VisaAppointment>> FetchVisaAppointments(string apiUrl)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
                throw new Exception("API'ye erişim başarısız!");

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<VisaAppointment>>(json)
                ?? throw new Exception("Veri JSON formatında değil!");
        }


    }

}
