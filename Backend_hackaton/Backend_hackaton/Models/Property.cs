using System;
using System.Collections.Generic;

namespace Backend_hackaton.Models;

public partial class Property
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ModelProperty> ModelProperties { get; set; } = new List<ModelProperty>();
}
