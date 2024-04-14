using DbLibrary.Entities;
namespace DbLibrary.Entities
{
    public class Like
    {
        public long Id { get; set; }
        public virtual User Liker { get; set; }
        public virtual Book Book { get; set; }
    }
}
