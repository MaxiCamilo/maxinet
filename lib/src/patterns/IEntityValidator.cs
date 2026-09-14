using System.Diagnostics.CodeAnalysis;

namespace MaxiNet;

public interface IEntityValidator<T> 
{
    InvalidationResult<T>  Validate(T entity);
}

public static class EntityValidatorExtensions
{
    public static bool Validate<T>(this EntityValidator<T> validator, List<T> list, [NotNullWhen(false)]  out List<InvalidationResult<T>>? errors)
    {
        
        
    }
} 