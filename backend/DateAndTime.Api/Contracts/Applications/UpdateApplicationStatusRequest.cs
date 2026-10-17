using System.ComponentModel.DataAnnotations;
using DateAndTime.Api.Domain;

namespace DateAndTime.Api.Contracts.Applications;

public sealed class UpdateApplicationStatusRequest
{
    [EnumDataType(typeof(ApplicationStatus))]
    public ApplicationStatus Status { get; init; }
}
