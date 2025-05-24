using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentApi.Models;

public class PrescriptionMedicament
{
    public int IdMedicament { get; set; }
    
    public int IdPrescription { get; set; }
    
    public int? Dose { get; set; }
    
    [MaxLength(100)]
    public string Details { get; set; }
    
    [ForeignKey("IdMedicament")]
    public Medicament Medicament { get; set; }
    
    [ForeignKey("IdPrescription")]
    public Prescription Prescription { get; set; } 
}