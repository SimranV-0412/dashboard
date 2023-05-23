using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dashboard.Models
{
    public class ArticleModel
    {
        public Guid Id { get; set; }
       public string Title { get; set; }
        //public string Content { get; set; }
       public string Link { get; set; }
    }
}
