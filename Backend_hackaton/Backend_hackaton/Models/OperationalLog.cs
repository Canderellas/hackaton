using System;
using System.Collections.Generic;

namespace Backend_hackaton.Models;

public partial class OperationalLog
{
    public int Id { get; set; }

    public string Place { get; set; } = null!;

    public DateTimeOffset DateOperation { get; set; }

    public string Comment { get; set; } = null!;

    public Guid FkDevice { get; set; }

    public virtual Device FkDeviceNavigation { get; set; } = null!;
}
