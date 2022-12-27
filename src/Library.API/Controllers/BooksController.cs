using AutoMapper;
using Library.API.Dtos;
using Library.API.Entities;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
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


        [HttpGet("{bookId}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid bookId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, bookId);
            if (bookForAuthorFromRepo == null)
                return NotFound();
            return Ok(Mapper.Map<BookDto>(bookForAuthorFromRepo));
        }

        [HttpPost]
        public IActionResult CreateBookforAuthor(Guid authorId, [FromBody] BookCreationDto bookCreationDto)
        {
            if (bookCreationDto == null)
                return BadRequest();

            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookEntity = Mapper.Map<Book>(bookCreationDto);

            _libraryRepository.AddBookForAuthor(authorId, bookEntity);
            if (!_libraryRepository.Save())
                throw new Exception("Creating a book failed on save.");
            var bookToReturn = Mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute("GetBookForAuthor", new { authorId = bookToReturn.AuthorId, bookId = bookToReturn.Id }, bookToReturn);
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();
            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId,id);
            
            if(bookForAuthorFromRepo== null) return NotFound();

            _libraryRepository.DeleteBook(bookForAuthorFromRepo);
            if (!_libraryRepository.Save())
                throw new Exception($"Delete book {id} for author {authorId} failed on save.");
            return NoContent();
        }

        
        [HttpPut("{id}")]
        public IActionResult UpdateBook(Guid authorId, Guid id,[FromBody] BookUpdateDto book)
        {
            if(book == null)return BadRequest();

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
        
        
        [HttpPatch("{bookId}")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId,Guid bookId, [FromBody] JsonPatchDocument<BookUpdateDto> patchDocument ) 
        {
            if(patchDocument ==null) return BadRequest();
            if (!_libraryRepository.AuthorExists(authorId)) return NotFound();

            var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId,bookId);
            if (bookFromRepo == null)
            {
                var bookDto = new BookUpdateDto();
                patchDocument.ApplyTo(bookDto);

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

            patchDocument.ApplyTo(bookToPatch);

            //validation

            Mapper.Map(bookToPatch, bookFromRepo);
            _libraryRepository.UpdateBookForAuthor(bookFromRepo);
            if (!_libraryRepository.Save()) throw new Exception($"Patching Book {bookId} for author {authorId} failed on save.");
            return NoContent();
        }
    }
}