using AutoMapper;
using Library.API.Dtos;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly ILogger _logger;
        private readonly IUrlHelper _urlHelper;

        public BooksController(ILibraryRepository repository,
            ILogger<BooksController> logger,
            IUrlHelper urlHelper)
        {
            _libraryRepository = repository;
            _logger= logger;
            _urlHelper = urlHelper;
        }


        [HttpGet(Name = "GetBooksForAuthor")]
        public IActionResult GetBooks(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();
            var booksForAuthorFromRepo = _libraryRepository.GetBooksForAuthor(authorId);
            var booksForAuthor = Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);
            booksForAuthor = booksForAuthor.Select(book =>
                {
                    book = CreateLinksForBook(book);
                    return book;
                });
            var wrapper = new LinkedCollectionResourceWrapperDto<BookDto>(booksForAuthor);
            
            return Ok(CreateLinksForBooks(wrapper));
        }


        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
                return NotFound();
            var bookForAuthor = Mapper.Map<BookDto>(bookForAuthorFromRepo);
            return Ok(CreateLinksForBook(bookForAuthor));
        }

        [HttpPost]
        public IActionResult CreateBookforAuthor(Guid authorId, [FromBody] BookCreationDto bookCreationDto)
        {
            if (bookCreationDto == null)
                return BadRequest();
            if (bookCreationDto.Title == bookCreationDto.Description)
                ModelState.AddModelError(nameof(BookCreationDto),"The provided Discription Should be different form the Title");
            if(!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookEntity = Mapper.Map<Book>(bookCreationDto);

            _libraryRepository.AddBookForAuthor(authorId, bookEntity);
            if (!_libraryRepository.Save())
                throw new Exception("Creating a book failed on save.");
            var bookToReturn = Mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute("GetBookForAuthor",
                new { authorId = bookToReturn.AuthorId, id = bookToReturn.Id },
                CreateLinksForBook( bookToReturn));
        }


        [HttpDelete("{id}", Name ="DeleteBookForAuthor")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();
            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId,id);
            
            if(bookForAuthorFromRepo== null) return NotFound();

            _libraryRepository.DeleteBook(bookForAuthorFromRepo);
            if (!_libraryRepository.Save())
                throw new Exception($"Delete book {id} for author {authorId} failed on save.");
            _logger.LogInformation(100, $"Book {id} for author {authorId} was deleted");
            return NoContent();
        }

        
        [HttpPut("{id}",Name ="UpdateBookForAuthor")]
        public IActionResult UpdateBook(Guid authorId, Guid id,[FromBody] BookUpdateDto book)
        {
            if(book == null)return BadRequest();

            if (book.Description == book.Title)
                ModelState.AddModelError(nameof(BookUpdateDto), "The provider Description should be different from book Title");
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);


            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();
            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);


            if (bookForAuthorFromRepo == null)
            {
                //  If book Doesnt exist => you Simpley add it
                                // ** Upserting **

                var bookToAdd = Mapper.Map<Book>(book);
                bookToAdd.AuthorId = authorId;
                bookToAdd.Id = id;
                _libraryRepository.AddBookForAuthor(authorId,bookToAdd);
                if (!_libraryRepository.Save())
                    throw new Exception($"Upserting Book {id} for author {authorId} failed on save.");

                var bookToReturn =Mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute("GetBookForAuthor", new { authorId = bookToReturn.AuthorId, bookId = bookToReturn.Id }, bookToReturn);
            }
             
            Mapper.Map(book, bookForAuthorFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);
            
            if (!_libraryRepository.Save()) throw new Exception($"Can not Update Book {id} ");
            
            return NoContent();
        }
        
        
        [HttpPatch("{id}", Name ="PartiallyUpdateBookForAuthor")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId,Guid bookId, [FromBody] JsonPatchDocument<BookUpdateDto> patchDocument ) 
        {
            if(patchDocument ==null) 
                return BadRequest();
            if (!_libraryRepository.AuthorExists(authorId)) 
                return NotFound();

            var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId,bookId);
            
            //Creating new Book => case it's null
                //  Upserting
            if (bookFromRepo == null)
            {
                var bookDto = new BookUpdateDto();
                patchDocument.ApplyTo(bookDto,ModelState);

                //  Apply Valdition On  the DTO
                if (bookDto.Title == bookDto.Description)
                    ModelState.AddModelError(nameof(BookUpdateDto), "The provided description should be different from the title.");
                TryValidateModel(bookDto);


                //  Case the DTO is invalied

                if (!ModelState.IsValid)
                    return new UnprocessableEntityObjectResult(ModelState);
                
                // Case DTO isValied
                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd.AuthorId = authorId;


                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!_libraryRepository.Save()) throw new Exception($"Upserting Book {bookToAdd.AuthorId} for author {authorId} failed on save.");

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor",
                    new {authorId = authorId, bookId = bookToReturn.Id}, bookToReturn);
            }
                        
            //Apply Patch Document

            var bookToPatch = Mapper.Map<BookUpdateDto>(bookFromRepo);

            // Any Errors in patch Document will make the ModelState Invalied
            
            patchDocument.ApplyTo(bookToPatch, ModelState);

            if (bookToPatch.Title == bookToPatch.Description)
            {
                ModelState.AddModelError(nameof(BookUpdateDto), "The provided description should be different from the title.");
            }

            //Try Validate BookUpdateDto Before Patching it

            TryValidateModel(bookToPatch);

            //check if JsonPatchDocument is valied
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);


            patchDocument.ApplyTo(bookToPatch);

            //validation

            Mapper.Map(bookToPatch, bookFromRepo);
            _libraryRepository.UpdateBookForAuthor(bookFromRepo);
            if (!_libraryRepository.Save()) throw new Exception($"Patching Book {bookId} for author {authorId} failed on save.");
            return NoContent();
        }

        private BookDto CreateLinksForBook(BookDto book)
        {
            book.Links.Add(
                new LinkDto(_urlHelper.Link("GetBookForAuthor",
                new { id = book.Id }),
                "self",
                "GET"));

             book.Links.Add(
                new LinkDto(_urlHelper.Link("DeleteBookForAuthor",
                new { id = book.Id }),
                "delete_book",
                "DELETE"));

             book.Links.Add(
                new LinkDto(_urlHelper.Link("UpdateBookForAuthor",
                new { id = book.Id }),
                "update_book",
                "PUT"));
             book.Links.Add(
                new LinkDto(_urlHelper.Link("PartiallyUpdateBookForAuthor",
                new { id = book.Id }),
                "partially_update_book",
                "PATCH"));


            return book;
        }

        private  LinkedCollectionResourceWrapperDto<BookDto> CreateLinksForBooks(
            LinkedCollectionResourceWrapperDto<BookDto> bookWrapper)
        {
           bookWrapper.Links.Add(
                new LinkDto(_urlHelper.Link("GetBooksForAuthor", new { }),
                "self",
                "GET"));
            return bookWrapper;
        }
    }
}