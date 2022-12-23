using Library.API.Dtos;
using Library.API.Helpers;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        public AuthorsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }
        [HttpGet]
        public IActionResult GetAuthors()
        {   
           var authorsFromRepo =  _libraryRepository.GetAuthors();
            var authorsDtos = new List<AuthorDto>();
            foreach (var author in authorsFromRepo)
            {
                authorsDtos.Add(new AuthorDto
                {
                    Id = author.Id,
                    Name = $"{author.FirstName} {author.LastName}",
                    Age = author.DateOfBirth.GetCurrentAge(),
                    Genre =author.Genre
                }) ;
            }
            return new JsonResult(authorsDtos);
        }

    }
}