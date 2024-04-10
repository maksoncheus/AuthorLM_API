namespace DbLibrary.Data.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public virtual User Author { get; set; }
        public virtual Book Book { get; set; }
        public string Text { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
