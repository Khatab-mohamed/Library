using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace Library.API.Helpers
{
    public static class ObjectExtensions
    {
        public static ExpandoObject ShapeData<TSource>(this TSource source, string fields) 
        {
        
            if (source == null) throw new ArgumentNullException("Source");
        
            var dataShapedObject = new ExpandoObject();
            
            // Case No Fields Specified in the query string 
            if (string.IsNullOrWhiteSpace(fields))
            {
                // All public properities should be Returned
                var properityInfos = typeof(TSource)
                    .GetProperties( BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                foreach (var properityInfo in properityInfos)
                {
                    var properityValue = properityInfo.GetValue(source);

                    ((IDictionary<string, object>)dataShapedObject).Add(properityInfo.Name, properityValue);
                }
                return dataShapedObject;
            }

            var fieldsAfterSplit = fields.Split(',');
            
            foreach (var field in fieldsAfterSplit)
            {
                var properityName = field.Trim();

                var properityInfo  = typeof(TSource)
                    .GetProperty(properityName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (properityInfo == null)
                    throw new Exception($"properity {properityName} wasn't found on {typeof(TSource)}");
                var properityValue = properityInfo.GetValue(source);

                ((IDictionary<string, object>)dataShapedObject).Add(properityInfo.Name, properityValue);

            }
            return dataShapedObject;


        }
    }
}
