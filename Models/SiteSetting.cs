using System.ComponentModel.DataAnnotations;

namespace FirstBloom.Models
{
    public class SiteSetting
    {
        public int Id { get; set; }

        public string AcademyName { get; set; }

        public string Logo { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }

        public string Facebook { get; set; }

        public string Instagram { get; set; }

        public string YouTube { get; set; }

        public string FooterDescription { get; set; }
    }
}