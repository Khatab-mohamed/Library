using System.Reflection;

namespace Library.API.Services
{
    public class TypeHelperService:ITypeHelperService
    {
        public bool TypeHasProperities<T>(string fields)
        {
             if(string.IsNullOrWhiteSpace(fields)) return true;

            var fieldsAfterSplit = fields.Split(',');
            
            foreach (string field in fieldsAfterSplit)
            {
                var properityName = field.Trim();
                
                //  Using Reflection to check if the properity can be found on T

                var properityInfo = typeof(T)
                    .GetProperty(properityName, BindingFlags.IgnoreCase |BindingFlags.Public | BindingFlags.Instance);
                
                
                if (properityInfo == null) return false;
            }
            return true;
        }
    }
}
