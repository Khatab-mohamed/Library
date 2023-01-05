using Library.API.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Library.API.Helpers
{
    public static class IQueryableExtensions
    {
        // It's an extension method
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy,
            Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if(mappingDictionary == null)
                throw new ArgumentNullException("mappingDictionary");
           
            if (string.IsNullOrWhiteSpace(orderBy))
                return source;

            // Ther orderBy string is separated by "," so we split it.
            var orderAfterSplit = orderBy.Split(',');

            //  apply each orderby clause in revese order - otherwise,
            //  The IQueryable will be ordered in the worng order
            foreach (var orderbyClause in orderAfterSplit.Reverse())
            {
                //  Trim the orderByClause, as it might contain leading 
                //  or trailing spaces. Can't trim the var in foreach,
                //  descending, otherwise ascending
                var trimmedOrderByClause = orderbyClause.Trim();

                //  if the sort option ends with "desc", we order
                //  get the property name to look for in the mapping
                var orderDescending = trimmedOrderByClause.EndsWith(" desc");

                // Remove "asc" or "desc" from the orderBylcause, so we
                // get the property name to look for in the mapping dictionary
                var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
                
                var propertyName = indexOfFirstSpace == -1 ? 
                    trimmedOrderByClause : trimmedOrderByClause.Remove(indexOfFirstSpace);

                // find the matching property
                if (!mappingDictionary.ContainsKey(propertyName))
                    throw new ArgumentException($"Key mapping for {propertyName} is missing");
               
                // Get PropertyMappingValue
                var propertyMappingValue = mappingDictionary[propertyName];
                
                if (propertyMappingValue == null)
                    throw new ArgumentNullException("propertyMappingValue"); 

                //  Run through the property names in revrse 
                //  so the ordetBy clauses are applied in correct order
                foreach (var destinationProperty in propertyMappingValue.DestinationProperties.Reverse())
                {
                    //  Revert sort order if necessary
                    if (propertyMappingValue.Revert)
                    {
                        orderDescending = !orderDescending;
                    }

                    source = source.OrderBy(destinationProperty + (orderDescending ?
                        " descending" : " ascending"));
                }
            }
            return source;

        }
    }
}
