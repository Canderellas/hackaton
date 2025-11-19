using System;
using System.Collections.Generic;

namespace Backend_hackaton.Models;

public partial class Device
{
    public Guid Id { get; set; }

    public int FkModel { get; set; }

    public virtual ICollection<DevicePropertyValue> DevicePropertyValues { get; set; } = new List<DevicePropertyValue>();

    public virtual Model FkModelNavigation { get; set; } = null!;

    public virtual ICollection<OperationalLog> OperationalLogs { get; set; } = new List<OperationalLog>();
}
