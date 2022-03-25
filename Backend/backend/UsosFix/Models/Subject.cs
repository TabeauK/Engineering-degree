using System.Collections.Generic;
using UsosFix.ViewModels;

namespace UsosFix.Models;

public class Subject
{
    public int Id { get; set; }
    public LanguageString Name { get; set; }
    public ICollection<Group> Groups { get; set; }
    public ICollection<Exchange> Exchanges { get; set; }
    public Semester Semester { get; set; }
}