namespace Ordering.Application.Exceptions
{
    public class NotFoundExpection : ApplicationException
    {
        public NotFoundExpection(string name, object key)
            : base($"Entity \"{name}\" ({key}) was not found")
        {
        }
    }
}
