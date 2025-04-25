namespace bookme_backend.BLL.Exceptions
{
    public class EntityDuplicatedException: Exception
    {
        public EntityDuplicatedException(string? message) : base(message) { }
    }
}
