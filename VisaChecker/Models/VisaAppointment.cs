using Newtonsoft.Json;

namespace VisaChecker.Models
{
    public record VisaAppointment(
     [property: JsonProperty("source_country")] string SourceCountry,
     [property: JsonProperty("mission_country")] string MissionCountry,
     [property: JsonProperty("center_name")] string CenterName,
     [property: JsonProperty("visa_subcategory")] string VisaSubCategory,
     [property: JsonProperty("appointment_date")] DateTime? AppointmentDate,
     [property: JsonProperty("book_now_link")] string BookNowLink);

}
