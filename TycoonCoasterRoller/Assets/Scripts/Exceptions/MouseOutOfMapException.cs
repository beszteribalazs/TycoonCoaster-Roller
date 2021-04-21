using System;

public class MouseOutOfMapException : Exception
{
    public MouseOutOfMapException(string message) : base(message)
    {
    }
}