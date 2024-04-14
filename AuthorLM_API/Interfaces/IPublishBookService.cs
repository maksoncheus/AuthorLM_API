using DbLibrary.Entities;
using AuthorLM_API.ViewModels;

namespace AuthorLM_API.Interfaces
{
    public interface IPublishBookService
    {
        Task<Book> PublishBookAsync(PublishBookViewModel publishBookRequest, User author, Genre genre);
    }
}
