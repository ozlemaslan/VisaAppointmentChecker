**Telegram da bot oluştururken yapılacak adımlar :**

Telegram uygulamasını açın ve @BotFather hesabına girin.
Hesaba /newbot yazıp Gönder'e basın. Sizden bot adı girmenizi isteyecek herhangi ad verebilirsiniz. Örn: ozlemVisaBot
Daha sonra size karmaşık değerler içeren bir mesaj gelecek. Örn: Use this token to access the HTTP API:63xxxxxx71:AAFoxxxxn0hwA-2TVSxxxNf4c gibi.
Burada bulunan 63xxxxxx71:AAFoxxxxn0hwA-2TVSxxxNf4c değeri sizin Bot token bot id değeriniz olmuş oluyor.
 
      private static readonly string botToken = "your-botid";  -> Kodda Bot id değerini buraya yazın.

Şimdi geldik chat-id değerini bulmaya.
Öncelikle telegramdan oluşturduğunuz isim verdiğiniz(ozlemVisaBot) botunuza girin hello veya herhangibir mesaj yazıp gönderin.
Daha sonra https://api.telegram.org/bot{our_bot_token}/getUpdates urline yukarıda elde ettiğimiz botid değerini {our_bot_token} olan yere yazın ve browserda çalıştırın.
Json şeklinde size veriler gelecek.Bu jsonda yazdığınız mesajı(Hello) göreceksiniz. Oradaki chat altında bulunan id değeri sizin chat-id değerinizdir.
örn: "chat": {
          "id": **XXXXXXX**,
           ......
          }

      private static readonly string chatId = "your-chatid"; -> Yukarıdaki chat-id değerini buraya yazın.
      
Artık vize telegram botunuz hazır. Kod çalıştıkça filtrelediğiniz ve uygun vize randevularını bulup bu bot üzerinden size mesaj gelecektir.
Telegram bot için kaynak : https://gist.github.com/nafiesl/4ad622f344cd1dc3bb1ecbe468ff9f8a 


