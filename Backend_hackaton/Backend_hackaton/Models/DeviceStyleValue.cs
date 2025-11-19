using System;
using System.Collections.Generic;

namespace Backend_hackaton.Models;

public partial class DeviceStyleValue
{
    public int Id { get; set; }

    public int FkDevicePropertyValue { get; set; }

    public int FkStyleProperty { get; set; }

    public string Value { get; set; } = null!;

    public virtual DevicePropertyValue FkDevicePropertyValueNavigation { get; set; } = null!;

    public virtual StyleProperty FkStylePropertyNavigation { get; set; } = null!;
}
