namespace LibSirius.SSH;

public class SshException : Exception
{
    public SshException(string message)
        : base(message)
    { }
}
