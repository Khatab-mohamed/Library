using System;
using System.ComponentModel.DataAnnotations;

namespace Library.API.Dtos
{
    public class BookUpdateDto : BookManipulationDto
    {
        [Required (ErrorMessage ="You should fill out a description")]
        public override string Description
        {
            get {
                return base.Description;
            }
            set { base.Description = value; }
        } 
    }

}