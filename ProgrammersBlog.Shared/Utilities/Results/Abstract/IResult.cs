using ProgrammersBlog.Shared.Utilities.Results.ComplexType;
using System;


namespace ProgrammersBlog.Shared.Utilities.Results.Abstract
{
    public interface IResult
    {
        public ResultStatus ResultStatus { get; set; }
        public string  Message { get; }
        public Exception Exception { get; }


     
    }
}
