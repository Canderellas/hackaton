using System;
using System.Collections.Generic;

namespace Backend_hackaton.Models;

public partial class StyleProperty
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<DeviceStyleValue> DeviceStyleValues { get; set; } = new List<DeviceStyleValue>();
}
