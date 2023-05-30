namespace Dashboard.Models
{
    public class ContentModel
    {
         public Guid Id { get; set; }
         public string Title { get; set; }
         public string Content { get; set; }
        public string PdfFileName { get; internal set; }
    }
}
