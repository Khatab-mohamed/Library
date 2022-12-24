using AutoMapper;
using Library.API.Dtos;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        public BooksController(ILibraryRepository repository)
        {
            _libraryRepository = repository;
        }
        
        [HttpGet]
        public IActionResult GetBooks(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();
            var booksForAuthorFromRepo = _libraryRepository.GetBooksForAuthor(authorId);
            var booksForAuthor = Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);
            return Ok(booksForAuthor);
        }

        [HttpGet("{bookId}")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid bookId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();
            
            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId,bookId);
            if (bookForAuthorFromRepo == null)
                return NotFound();
            return Ok(Mapper.Map<BookDto>(bookForAuthorFromRepo));
        }
    }
}