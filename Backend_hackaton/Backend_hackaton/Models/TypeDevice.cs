using System;
using System.Collections.Generic;

namespace Backend_hackaton.Models;

public partial class TypeDevice
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Model> Models { get; set; } = new List<Model>();
}
