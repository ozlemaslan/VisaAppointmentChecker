using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text;
using VisaChecker.Models;

namespace VisaChecker.Services
{
    public class RabbitMqService : IDisposable
    {
        private IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private object lock_object = new();
        private int RetryCount = 5;
        private bool _disposed;

        public static string ExchangeName = "VisaDirectExchange";
        public static string RoutingVisa = "visa-route";
        public static string QueueName = "queue-visa";

        // https://gist.github.com/nafiesl/4ad622f344cd1dc3bb1ecbe468ff9f8a buradan telegram bot yaptım.

        private static readonly string botToken = "your-botid";
        private static readonly string chatId = "your-chatid";

        public bool IsConnected => _connection != null && _connection.IsOpen;

        public RabbitMqService(IConnectionFactory connectionFactory)
        {
            _connectionFactory=connectionFactory;

            //rabbitmqya bağlantı 
            if (!IsConnected)
            {
                bool isConnected = TryConnect();
                if (isConnected)
                {
                    _channel = CreateModel();
                }
            }
            else _channel = CreateModel();


            _channel.ExchangeDeclare(ExchangeName, type: "direct", true, false);
            _channel.QueueDeclare(QueueName, true, false, false, null);
            _channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingVisa);

        }

        public IModel CreateModel()
        {
            return _connection.CreateModel();
        }

        public bool TryConnect()
        {
            lock (lock_object) // lock
            {
                var policy = Policy.Handle<SocketException>().Or<BrokerUnreachableException>()
                    .WaitAndRetry(this.RetryCount, r => TimeSpan.FromSeconds(Math.Pow(2, r)), (ex, ts) =>
                    {

                    });

                policy.Execute(() =>
                {
                    _connection = _connectionFactory.CreateConnection();
                });

                if (IsConnected && !_disposed) return true;
                return false;
            }
        }

        public void SendtoRabbitMq(List<VisaAppointment> appointments)
        {
            Console.WriteLine("RabbitMQ ile bağlantı yapıldı.");

            var message = appointments;

            var messageBody = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(messageBody);

            _channel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: null, body: body);

            Console.WriteLine("RabbitMQ ya mesaj gönderildi...");
        }

        public void ListentoRabbitMqAndSendtoTelegram()
        {
            Console.WriteLine("RabbitMQ ile bağlantı yapıldı. Mesajlar dinleniyor...");

            // Mesaj dinleyici tanımlama
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var appointmentData = JsonConvert.DeserializeObject<List<VisaAppointment>>(message);
                if (appointmentData == null)
                {
                    Console.WriteLine("Rabbitmq daki message dinlenip deserilize edilirken hata alındı...");
                }
                else
                {
                    foreach (var appointment in appointmentData)
                    {
                        string telegramMessage = $@"{appointment.SourceCountry} ülkesinden {appointment.MissionCountry} ülkesine {appointment.AppointmentDate} tarihinde {appointment.VisaSubCategory} kategorili vize randevusu bulunmuştur.Lütfen aşağıdaki bağlantıdan giriş yapın: {appointment.BookNowLink}";

                        Console.WriteLine("Telegram'a mesaj gönderiliyor...");
                        // Mesajı Telegram'a gönder
                        await SendMessageToTelegramAsync(telegramMessage);
                        Console.WriteLine("Telegram'a mesaj gönderildi.");
                    }
                }

            };

            _channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);

            // Programın devam etmesini sağlamak için kullanıcıdan giriş bekleyin
            Console.WriteLine("Çıkmak için 'Enter' tuşuna basın.");
            Console.ReadLine();

            Console.WriteLine("RabbitMQ ya mesaj gönderildi...");
        }

        public void Dispose()
        {
            _disposed = true;
            _connection.Dispose();

            Console.WriteLine("RabbitMQ ile bağlantı gitti...");
        }

        static async Task SendMessageToTelegramAsync(string message)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"https://api.telegram.org/bot{botToken}/sendMessage";
                var payload = new
                {
                    chat_id = chatId,
                    text = message
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Mesaj başarıyla gönderildi!");
                }
                else
                {
                    Console.WriteLine($"Hata: {response.StatusCode}");
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Yanıt: {responseContent}");
                }
            }
        }
    }
}
