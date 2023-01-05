using Library.API.Dtos;
using Library.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        // Dictionary  whith strings as Keys, PropertyMappingValue as a values.
        private Dictionary<string, PropertyMappingValue> _authorPropertyMapping = 
            new Dictionary<string, PropertyMappingValue> (StringComparer.OrdinalIgnoreCase)
        {
            {   "Id" , new PropertyMappingValue(new List<string>() { "Id"} ) },
            {   "Genre" , new PropertyMappingValue(new List<string>() { "Genre" }) },
            {   "Age" , new PropertyMappingValue(new List<string>() { "DateOfBirth"}, true) },
            {   "Name" , new PropertyMappingValue(new List<string>() {"FirstName","LastName"}) }
        };

        private IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

        // Constructor
        public PropertyMappingService()
        {
            propertyMappings.Add(new PropertyMapping<AuthorDto,Author>(_authorPropertyMapping));

        }



        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            //  get Matching mapping 
            var matchingMapping = propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();
           
            
            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First()._mappingDictionary;
            }
            throw new Exception($"Cannot find exact proprty mapping instance for <{typeof(TSource)} ");
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
                return true;

            //  The string is separated by ',' so we split it.
            var fieldsAfterSplit = fields.Split(',');

            //  Run through the fields clauses
            foreach (string field in fieldsAfterSplit)
            {
                //  Trim
                var trimmedField = field.Trim();

                //  Remove Everything After the first " " 
                //  If the Fields are comming from an orderBy string, this part
                //  must be ignored.

                var indexOfFirstSpace = trimmedField.IndexOf(' ');

                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedField : trimmedField.Remove(indexOfFirstSpace);


                if (!propertyMapping.ContainsKey(propertyName))
                    return false;

            }
            return true;
        }
    }
}