namespace bookme_backend.BLL.Exceptions
{
    public class ValidationException : Exception
    {
        public Dictionary<string, string> Errores { get; }

        public ValidationException(string message, Dictionary<string, string> errores) : base(message)
        {
            Errores = errores;
        }
    }
}
