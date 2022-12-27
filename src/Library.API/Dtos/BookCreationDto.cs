using System.ComponentModel.DataAnnotations;

namespace Library.API.Dtos
{
    public class BookCreationDto
    {
        [Required(ErrorMessage = "You should fill out a title")]
        [MaxLength(100, ErrorMessage = "The Title Shouldn't have more than 100 Charecters")]
        public string Title { get; set; }


        [MaxLength(500, ErrorMessage = "The Description Shouldn't have more than 500 Charecters")]
        public string Description{ get; set; }
    }

}