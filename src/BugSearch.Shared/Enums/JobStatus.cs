using System.ComponentModel;

namespace BugSearch.Shared.Enums;

public enum JobStatus
{
    None = 0,
    [Description("Aguardando")]
    Waiting = 1,
    [Description("Em execução")]
    Running = 2,
    [Description("Concluído")]
    Completed = 3,
    [Description("Cancelamento solicitado")]
    RequestedCancel = 4,
    [Description("Cancelado")]
    Canceled = 5,
    [Description("Erro")]
    Error = 6
}