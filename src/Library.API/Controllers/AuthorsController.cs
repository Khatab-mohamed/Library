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
        private readonly IUrlHelper _urlHelper;
        public AuthorsController(ILibraryRepository libraryRepository, IUrlHelper urlHelper)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
        }
        
        [HttpGet(Name ="GetAuthors")]
        public IActionResult GetAuthors(AuthorsResouceParameters authorsResouceParameters)
        {
            //  PagedList<Author>
            var authorsFromRepo =  _libraryRepository.GetAuthors(authorsResouceParameters);

            var previousPageLink = authorsFromRepo.HasPrevious ?
                CreateAuthorRsourceUri(authorsResouceParameters,
                ResourceUriType.PreviousPage) : null;

             var nextPageLink = authorsFromRepo.HasNext ?
                CreateAuthorRsourceUri(authorsResouceParameters,
                ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink= nextPageLink
            };

            Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));


            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

            return Ok(authors);

        }


        //Helper Method
        private string CreateAuthorRsourceUri(
            AuthorsResouceParameters authorsResouceParameters,
            ResourceUriType type)
        {
            switch(type)
            {
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAuthors", new
                    {
                        searchQuery = authorsResouceParameters.SearchQuery,
                        genre = authorsResouceParameters.Genre,
                        pageNumber = authorsResouceParameters.PageNumber + 1,
                        pageSize = authorsResouceParameters.PageSize
                    }); ;
                    break;
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors", new
                    {
                        searchQuery = authorsResouceParameters.SearchQuery,
                        genre = authorsResouceParameters.Genre,
                        pageNumber = authorsResouceParameters.PageNumber - 1,
                        pageSize = authorsResouceParameters.PageSize
                    }) ;
                    break;
                default: return _urlHelper.Link("GetAuthors", new
                {
                    searchQuery = authorsResouceParameters.SearchQuery,
                    genre = authorsResouceParameters.Genre,
                    pageNumber = authorsResouceParameters.PageNumber,
                    pageSize = authorsResouceParameters.PageSize
                
                });
                    
            }
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

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);

            if (authorFromRepo==null)
                return NotFound();
            _libraryRepository.DeleteAuthor(authorFromRepo);
            if (!_libraryRepository.Save())
                throw new Exception($"Deleting author {id} failed on save.");
            return NoContent();

        }
    }
}