using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.API.Helpers
{
    public class PagedList<T> : List<T> where T : class
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set;}
        public int TotalCount { get; private set;}
        public bool HasPrevious
        {
            get { return CurrentPage > 1; }
        }
        public bool HasNext
        {
             get { return CurrentPage < TotalPages; }

        }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;

            CurrentPage= pageNumber;
            
            TotalPages = (int)Math.Ceiling(count/(double)PageSize);
            
            AddRange(items);
            
        }
        
        // Create => Creates PagedList<T> for us

        public static PagedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}