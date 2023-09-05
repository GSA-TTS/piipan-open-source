namespace Piipan.Notifications.Models
{
    public class EmailModel
    {
        public List<string> ToList { get; set; }
        public List<string> ToCCList { get; set; }
        public List<string> ToBCCList { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }

        public string Body { get; set; }


        public EmailModel()
        {

        }
    }
}
