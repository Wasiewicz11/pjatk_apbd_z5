using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentApi.Models;

public class Doctor
{
    [Key]
    public int IdDoctor { get; set; }

    [MaxLength(100)]
    public string FirstName { get; set; }
    

    [MaxLength(100)]
    public string LastName { get; set; }
    
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; }
    
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}