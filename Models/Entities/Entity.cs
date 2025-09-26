using System.ComponentModel.DataAnnotations;

namespace StudyTests.Models.Entities;

public class Entity
{
    [Key]
    public int Id { get; set; }
}