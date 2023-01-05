using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace Library.API.Helpers
{
    public static class IEnumerableExtensions
    {


        public static IEnumerable<ExpandoObject> ShapeData<TSource>(
            this IEnumerable<TSource> source,
            string fields)
        {

            if (source == null)
                throw new ArgumentNullException("source");

            // Create a list to hold our ExpandoOpjects
            
            var expandoOpjects = new List<ExpandoObject>();

            //  Create a list with PropertyInfo objects on TSource. Reflection is
            //  Expensive, so Rather than doing it for each object in the list, we do
            //  it once and reuse the results. After all, part of the reflection is on the
            //  type of the object (TSource), not on the instance.
            var properityInfoList = new List<PropertyInfo>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                //  All public properties should be in the expandoObject
                var propertyInfos = typeof(TSource)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance);

                properityInfoList.AddRange(propertyInfos);

            }
            else
            {
                //  only the public properties that match the fields should be
                //  in tne ExpandoOpject

                // the fields are separated by "," sow we spilt it.
                var filedsAfterSpilt = fields.Split(',');

                foreach (var field in filedsAfterSpilt)
                {
                    //  Trim each Fields, as it migth contain leadin
                    //  Or Trailing Spaces, Can't Trim the var in foreach,
                    //  so use another var.
                    var propertyName = field.Trim();
                    
                    //  
                    var properityInfo = typeof(TSource)
                        .GetProperty(propertyName,BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (properityInfo == null)
                        throw new Exception($"Property {propertyName} wasn't found on {typeof(TSource)}");

                    //  Add PropertyInfo to list
                    properityInfoList.Add(properityInfo);
                }
            }

            // Run through the source objects
            foreach (TSource sourceObject in source)
            {
                //  Create an ExpandoOpject that will hold the 
                //  Selected Properties & values 

                var dataShapedObject = new ExpandoObject();

                //  Get The value of each property we have to return. for that,
                //  we run through the list
                foreach (var properityInfo in properityInfoList)
                {
                    //  GetValue returns the value of the property on the source object
                    var properityValue = properityInfo.GetValue(sourceObject);
                    
                    //  Add the field to ExpandoObject
                    ((IDictionary<string,Object>)dataShapedObject).Add(properityInfo.Name, properityValue);
                }

                //  Add the expandoOpject to the list
                expandoOpjects.Add(dataShapedObject);
            }
            
            //  Return the list

            return expandoOpjects;
        }

    }
}
