namespace Library.API.Helpers
{
    public class AuthorsResouceParameters
    {
        const int maxAuthorPageSize = 20;
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize { 
            get {
                return _pageSize;
            }
            set
            {
               _pageSize = (value > maxAuthorPageSize ) ? maxAuthorPageSize: value;
            }
        }
        public string Genre { get; set; }
    }
}
