using System;

public class NotValidCellException : Exception
{
    public NotValidCellException(string message) : base(message)
    {
    }
}