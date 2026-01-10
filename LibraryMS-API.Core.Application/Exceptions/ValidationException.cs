namespace LibraryMS_API.Core.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public List<string> Errors { get; set; }

        public ValidationException() : base("Error occurs")
        {
            Errors = [];
        }

        //public ValidationException(IEnumerable<ValidationFailure> failures) : this()
        //{
        //    foreach (var failure in failures)
        //    {
        //        Errors.Add(failure.ErrorMessage);
        //    }
        //}

    }
}
