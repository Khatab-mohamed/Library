using AutoMapper;
using Library.API.Dtos;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public IActionResult CreateAuthorCollection([FromBody]IEnumerable<AuthorCreationDto> authorCollection)
        {
            if (authorCollection == null)
            { return BadRequest(); }

            var authorEntities = Mapper.Map<IEnumerable<Author>>(authorCollection);
            var authors = authorEntities;


            foreach (var author in authorEntities)
            {
                _libraryRepository.AddAuthor(author);
            }
            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an Author collection failed on save.");
            }
            
            
            var authorCollectionToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            var idsAsString = string.Join(",", authorCollectionToReturn.Select(a=>a.Id));
            
            return CreatedAtRoute("GetAuthorCollection",new { ids= idsAsString},authorCollectionToReturn);
        }

        [HttpGet("{ids}",Name ="GetAuthorCollection")]
        public IActionResult GetAuthorsCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if(ids==null)
                return BadRequest();
           var authorsEntities =  _libraryRepository.GetAuthors(ids);
            
            if(ids.Count() != authorsEntities.Count())
                return NotFound();
            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorsEntities);
            return Ok(authorsToReturn);
        }

    }
}