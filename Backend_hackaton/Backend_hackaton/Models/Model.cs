using System;
using System.Collections.Generic;

namespace Backend_hackaton.Models;

public partial class Model
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int FkType { get; set; }

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    public virtual TypeDevice FkTypeNavigation { get; set; } = null!;

    public virtual ICollection<ModelProperty> ModelProperties { get; set; } = new List<ModelProperty>();
}
