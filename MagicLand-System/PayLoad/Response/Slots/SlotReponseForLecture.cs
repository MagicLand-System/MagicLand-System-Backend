namespace MagicLand_System.PayLoad.Response.Slots
{
    public class SlotReponseForLecture
    {
        public List<string>? Slot { get; set; } = new List<string>();
        public List<(string, string)>? Time { get; set; } = new List<(string, string)>();
    }
}
