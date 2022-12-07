using System;

namespace Keyfactor.Extensions.Orchestrator.Imperva
{
    internal class ImpervaException : ApplicationException
    {
        public ImpervaException(string message) : base(message)
        { }

        public ImpervaException(string message, Exception ex) : base(message, ex)
        { }

        public static string FlattenExceptionMessages(Exception ex, string message)
        {
            message += ex.Message + Environment.NewLine;
            if (ex.InnerException != null)
                message = FlattenExceptionMessages(ex.InnerException, message);

            return message;
        }
    }
}
