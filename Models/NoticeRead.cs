namespace FirstBloom.Models
{
    public class NoticeRead
    {
        public int Id { get; set; }

        public int NoticeId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public DateTime ReadAt { get; set; } = DateTime.Now;
    }
}