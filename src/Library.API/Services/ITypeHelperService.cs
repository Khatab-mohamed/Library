namespace Library.API.Services
{
    public interface ITypeHelperService 
    {
        bool TypeHasProperities<T>(string fields);

    }
}
