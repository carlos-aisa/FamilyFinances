using FamilyFinances.Domain.Common;

namespace FamilyFinances.Application.Operations.BackupRestore.Exceptions;

public sealed class BackupOperationInProgressException : Exception
{
    public BackupOperationInProgressException()
        : base("A backup/restore operation is already in progress.")
    {
    }
}

public sealed class IncompatibleBackupPackageException : DomainException
{
    public IncompatibleBackupPackageException(string message)
        : base(message)
    {
    }
}

public sealed class BackupRestoreApplyException : DomainException
{
    public BackupRestoreApplyException(string message)
        : base(message)
    {
    }
}
