using System.Text.Json.Serialization;

namespace ScheduleDay.Models
{
	public class GoogleEvent
	{
		public required string Id { get; set; }
		public string Summary { get; set; } = "";
		public string Description { get; set; } = "";
		public GoogleEventDateTime Start { get; set; } = new();
	}

	public class GoogleEventDateTime
	{
		public DateTime? DateTime { get; set; }
		public DateTime? Date { get; set; }
		[JsonIgnore]
		public DateTime EventDate => DateTime ?? Date ?? default;
	}

	public class GoogleCalendarResponse
	{
		public List<GoogleEvent> Items { get; set; } = [];
	}

}
