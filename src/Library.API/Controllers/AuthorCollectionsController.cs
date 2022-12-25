using AutoMapper;
using Library.API.Dtos;
using Library.API.Entities;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Library.API.Controllers
{
    [Route("api/authorCollections")]
    public class AuthorsCollectionsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public AuthorsCollectionsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }
        [HttpPost]
        public IActionResult CreateAuthorCollection(IEnumerable<AuthorCreationDto> authorCollection)
        {
            if (authorCollection == null)
                return BadRequest();
             
            var authorEntities = Mapper.Map<IEnumerable<Author>>(authorCollection);
           
            
            foreach (var author in authorEntities)
            {
                _libraryRepository.AddAuthor(author);
            }
            if (!_libraryRepository.Save())
                throw new Exception("Creating an Author collection failed on save.");
            return Ok();
        }

    }
}