using DbLibrary.Data.Entities;
using Swashbuckle.AspNetCore.Annotations;
namespace AuthorLM_API.ViewModels
{
    public class PublishBookViewModel
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int GenreId { get; set; }
        public IFormFile? CoverImage { get; set; }
        [SwaggerSchema(ReadOnly = true)]
        public string? CoverImagePath { get; set; }
        public IFormFile Content { get; set; }
        [SwaggerSchema(ReadOnly = true)]
        public string? ContentPath { get; set; } = null!;
    }
}
