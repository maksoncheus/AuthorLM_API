namespace DbLibrary.Data.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public virtual User Author { get; set; } = null!;
        public virtual Genre Genre { get; set; } = null!;
        public int Rating { get; set; }
        public DateOnly PublicationDate { get; set; }
        public string CoverImagePath { get; set; }
        public string ContentPath { get; set; } = null!;
    }
}
