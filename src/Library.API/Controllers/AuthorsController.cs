using AutoMapper;
using Library.API.Dtos;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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

            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

            return Ok(authors);
        }
        
        [HttpGet("{id}",Name ="GetAuthor")]
        public IActionResult GetAuthors(Guid id)
        {
            if (!_libraryRepository.AuthorExists(id))
                return NotFound();
            var authorFromRepo = _libraryRepository.GetAuthor(id);

            if (authorFromRepo == null)
                return NotFound();
           var authors = Mapper.Map<AuthorDto>(authorFromRepo);

            return new JsonResult(authors);
        }

        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AuthorCreationDto authorDto)
        {
            if (authorDto == null)
                return BadRequest();
            var authorEntity = Mapper.Map<Author>(authorDto);
            _libraryRepository.AddAuthor(authorEntity);
            if (!_libraryRepository.Save())
                throw new Exception("Create an Author is failed on save.");
            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);

            return CreatedAtRoute("GetAuthor",new{ authorEntity.Id},authorToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id) 
        {
            if (_libraryRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }
            return BadRequest();
        }
    }
}