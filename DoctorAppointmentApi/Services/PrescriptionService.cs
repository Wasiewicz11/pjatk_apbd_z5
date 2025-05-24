using DoctorAppointmentApi.Data;
using DoctorAppointmentApi.DTOs;
using DoctorAppointmentApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentApi.Services;

public interface IPrescriptionService
{
    Task<int> AddPrescriptionAsync(AddPrescriptionRequest request);
    Task<PatientDetailsDto> GetPatientDetailsAsync(int id);
}

public class PrescriptionService : IPrescriptionService
{
    private readonly DatabaseContext _context;
    
    public PrescriptionService(DatabaseContext context)
    {
        _context = context;
    }
    
    public async Task<int> AddPrescriptionAsync(AddPrescriptionRequest request)
    {

        if (request.DueDate < request.Date)
            throw new ArgumentException("Okres przyjmowania leków musi wynosić minimum 1 dzień");
        
        if (request.Medicaments.Count > 10)
            throw new ArgumentException("Przekroczono limit (10) leków na recepcie. Aby przepisać nowe leki wystaw nową receptę");

        var presentMedicamentsIds = await _context.Medicaments.Select(m => m.IdMedicament).ToListAsync();
        
        foreach (var medicament in request.Medicaments)
        {
            if (!presentMedicamentsIds.Contains(medicament.IdMedicament))
                throw new ArgumentException($"Lek {medicament.Description} nie istnieje");

        }
        
        var patient = request.Patient.IdPatient.HasValue 
            ? await _context.Patients.FindAsync(request.Patient.IdPatient.Value)
            : null;
            
        if (patient == null)
        {
            patient = new Patient
            {
                FirstName = request.Patient.FirstName,
                LastName = request.Patient.LastName,
                Birthdate = request.Patient.Birthdate
            };
            
            _context.Patients.Add(patient);
            
            await _context.SaveChangesAsync();
        }
        
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.IdDoctor == request.IdDoctor) 
                     ?? throw new ArgumentException($"Lekarz o ID={request.IdDoctor} nie istnieje.");
       
        
        var prescription = new Prescription
        {
            Date = request.Date,
            DueDate = request.DueDate,
            IdPatient = patient.IdPatient,
            IdDoctor = request.IdDoctor,
            PrescriptionMedicaments = request.Medicaments.Select(m => new PrescriptionMedicament
            {
                IdMedicament = m.IdMedicament,
                Dose = m.Dose,
                Details = m.Description ?? "Stosować według zaleceń lekarza"
            }).ToList()
        };
        
        _context.Prescriptions.Add(prescription);
        
        await _context.SaveChangesAsync();

        return 1;
    }
    
    public async Task<PatientDetailsDto> GetPatientDetailsAsync(int id)
    {
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.IdPatient == id)
            ?? throw new ArgumentException($"Pacjent o ID={id} nie istnieje.");
        
        
        return new PatientDetailsDto
        {
            IdPatient = patient.IdPatient,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Birthdate = patient.Birthdate,
            Prescriptions = patient.Prescriptions
                .OrderBy(p => p.DueDate)
                .Select(p => new PrescriptionDto
                {
                    IdPrescription = p.IdPrescription,
                    Date = p.Date,
                    DueDate = p.DueDate,
                    Doctor = new DoctorDto
                    {
                        IdDoctor = p.Doctor.IdDoctor,
                        FirstName = p.Doctor.FirstName,
                        LastName = p.Doctor.LastName,
                        Email = p.Doctor.Email
                    },
                    Medicaments = p.PrescriptionMedicaments
                        .Select(pm => new MedicamentDto
                        {
                            IdMedicament = pm.IdMedicament,
                            Description = pm.Details,
                            Dose = pm.Dose
                        }).ToList()
                }).ToList()
        };
    }
}
