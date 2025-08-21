using System;

namespace Api.Domain.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
