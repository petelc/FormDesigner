using System;

namespace FormDesignerAPI.UseCases.Exceptions;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base() { }

}
