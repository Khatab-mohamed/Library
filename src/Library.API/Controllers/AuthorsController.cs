using AutoMapper;
using Library.API.Dtos;
using Library.API.Helpers;
using Library.API.Services;
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
        
        [HttpGet("{id}")]
        public IActionResult GetAuthors(Guid id)
        {
            
            var authorFromRepo = _libraryRepository.GetAuthor(id);

            if (authorFromRepo == null)
                return NotFound();
           var authors = Mapper.Map<AuthorDto>(authorFromRepo);

            return new JsonResult(authors);
        }



    }
}