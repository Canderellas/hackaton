using System;
using System.Collections.Generic;

namespace Backend_hackaton.Models;

public partial class ModelProperty
{
    public int Id { get; set; }

    public int FkModel { get; set; }

    public int FkProperty { get; set; }

    public virtual ICollection<DevicePropertyValue> DevicePropertyValues { get; set; } = new List<DevicePropertyValue>();

    public virtual Model FkModelNavigation { get; set; } = null!;

    public virtual Property FkPropertyNavigation { get; set; } = null!;
}
