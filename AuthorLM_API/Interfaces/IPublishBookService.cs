using DbLibrary.Data.Entities;
using AuthorLM_API.ViewModels;

namespace AuthorLM_API.Interfaces
{
    public interface IPublishBookService
    {
        //Task SaveBookCoverImageAsync(PublishBookViewModel publishBookRequest);
        //Task SaveBookContentAsync(PublishBookViewModel publishBookRequest);
        Task<Book> PublishBookAsync(PublishBookViewModel publishBookRequest, User author, Genre genre);
    }
}
