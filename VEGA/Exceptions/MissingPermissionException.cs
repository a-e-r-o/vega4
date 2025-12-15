using NetCord.Services;

namespace Exceptions;

public class MissingPermissionException : Exception
{
    public MissingPermissionsResult MissingPerm {get; set;}

    public MissingPermissionException(MissingPermissionsResult missingPerm) 
        : base("Missing permission")
    {
        MissingPerm = missingPerm;
    }
}