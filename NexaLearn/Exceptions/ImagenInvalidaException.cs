namespace NexaLearn.Exceptions;

public sealed class ImagenInvalidaException : Exception
{
    public ImagenInvalidaException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
