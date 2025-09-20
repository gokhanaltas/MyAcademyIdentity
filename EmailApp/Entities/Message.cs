using EmailApp.Entities;

namespace IdentityEmailApp.Entities
{
    public class Message
    {
        public int? ReceiverId { get; set; }
        public AppUser? Receiver { get; set; }
        public int SenderId { get; set; }
        public AppUser Sender { get; set; }
        public int MessageId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime SendDate { get; set; }

        public bool IsRead { get; set; } // Okundu bilgisi için
        public bool IsDeleted { get; set; }// Silinen mesajlar için
        public bool IsImportant { get; set; } //Yıldız için

        public bool IsDraft { get; set; }
        public DateTime? DraftDate { get; set; }

    }
}
