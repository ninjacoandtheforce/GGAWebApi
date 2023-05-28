using System;
using System.Collections.Generic;

namespace GGAWebApi.Entities;

public partial class Product
{
    public int Id { get; set; }

    public string ProductName { get; set; }

    public string ProductDescription { get; set; }

    public decimal ProductPrice { get; set; }

    public string ProductUrl { get; set; }

    public string Username { get; set; }
}
