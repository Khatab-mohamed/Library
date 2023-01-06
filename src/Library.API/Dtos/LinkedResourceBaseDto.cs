using System.Collections.Generic;

namespace Library.API.Dtos
{
    public abstract class LinkedResourceBaseDto 
    {
        public List<LinkDto> Links { get; set; }
        = new List<LinkDto>();
    }
}
