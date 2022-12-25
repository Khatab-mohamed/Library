using System;

namespace Library.API.Dtos
{
    public class AuthorCreationDto
    {
        public string FirstName { get; set; } 
        public string LastName { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public string Genre { get; set; }
    }

}