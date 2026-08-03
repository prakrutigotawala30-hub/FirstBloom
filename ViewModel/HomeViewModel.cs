using FirstBloom.Models;

namespace FirstBloom.ViewModels
{
    public class HomeViewModel
    {
        public List<Programs> Programs { get; set; } = new();

        public List<Gallery> Galleries { get; set; } = new();

        public List<Blog> Blogs { get; set; } = new();

        public List<Testimonial> Testimonials { get; set; } = new();

        public SiteSetting SiteSetting { get; set; }
    }
}