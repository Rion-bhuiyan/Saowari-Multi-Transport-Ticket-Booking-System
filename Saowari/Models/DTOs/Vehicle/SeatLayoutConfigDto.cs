using System.Collections.Generic;

namespace Saowari.Models.DTOs.Vehicle
{
    public class SeatLayoutConfigDto
    {
        public string Mode { get; set; } = "visual"; 
        public int GridWidth { get; set; } = 5;
        public int GridHeight { get; set; } = 15;
        public bool IsDoubleDecker { get; set; } = false;
        public List<DeckConfigDto> Decks { get; set; } = new List<DeckConfigDto>();
    }

    public class DeckConfigDto
    {
        public int Level { get; set; }
        public string Name { get; set; } = "";
        public List<VisualSeatDto> Seats { get; set; } = new List<VisualSeatDto>();
    }

    public class VisualSeatDto
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public string SeatNumber { get; set; } = "";
        public int SeatClassId { get; set; }
    }

    public class SeatClassAssignmentDto
    {
        public string SeatNumber { get; set; } = "";
        public int SeatClassId { get; set; }
    }
}
