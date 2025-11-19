using System;
using System.Collections.Generic;

namespace Backend_hackaton.Models;

public partial class DevicePropertyValue
{
    public int Id { get; set; }

    public Guid FkDevice { get; set; }

    public int FkModelProperty { get; set; }

    public string Value { get; set; } = null!;

    public virtual ICollection<DeviceStyleValue> DeviceStyleValues { get; set; } = new List<DeviceStyleValue>();

    public virtual Device FkDeviceNavigation { get; set; } = null!;

    public virtual ModelProperty FkModelPropertyNavigation { get; set; } = null!;
}
