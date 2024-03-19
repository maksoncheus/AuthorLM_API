using AuthorLM_API.Data;
using AuthorLM_API.Data.Entities;
using AuthorLM_API.Helpers;
using AuthorLM_API.Interfaces;
using AuthorLM_API.ViewModels;
using System;

namespace AuthorLM_API.Services
{
    public class PublishBookService : IPublishBookService
    {
        private readonly IWebHostEnvironment _environment;
        public PublishBookService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        //Call after saving to wwwroot!!!
        public async Task<Book> PublishBookAsync(PublishBookViewModel publishBookRequest, User author, Genre genre)
        {
        DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            Book book = new() {
                Title = publishBookRequest.Title,
                Description = publishBookRequest.Description,
                Author = author,
                Genre = genre,
                PublicationDate = currentDate,
                CoverImagePath = publishBookRequest.CoverImagePath,
                ContentPath = publishBookRequest.ContentPath
            };
            await SaveBookContentAsync(publishBookRequest);
            await SaveBookCoverImageAsync
            return book;
        }

        private async Task SaveBookContentAsync(PublishBookViewModel publishBookRequest)
        {
            var uniqueFileName = FileHelper.GetUniqueFileName(publishBookRequest.Content.FileName);

            var uploads = Path.Combine(_environment.WebRootPath, "books", publishBookRequest.Title.ToString());

            var filePath = Path.Combine(uploads, uniqueFileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            await publishBookRequest.Content.CopyToAsync(new FileStream(filePath, FileMode.Create));

            publishBookRequest.ContentPath = filePath;
            return;
        }

        private async Task SaveBookCoverImageAsync(PublishBookViewModel publishBookRequest)
        {
            if(publishBookRequest.CoverImagePath == null)
            {
                publishBookRequest.CoverImagePath = Path.Combine(_environment.WebRootPath, "src", "images", "bookNoCover.png");
                return;
            }
            var uniqueFileName = FileHelper.GetUniqueFileName(publishBookRequest.CoverImage.FileName);

            var uploads = Path.Combine(_environment.WebRootPath, "books", publishBookRequest.Title.ToString());

            var filePath = Path.Combine(uploads, uniqueFileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            await publishBookRequest.CoverImage.CopyToAsync(new FileStream(filePath, FileMode.Create));

            publishBookRequest.CoverImagePath = filePath;
            return;
        }
    }
}
