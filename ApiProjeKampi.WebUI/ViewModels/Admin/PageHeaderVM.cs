namespace ApiProjeKampi.WebUI.ViewModels.Admin
{
    public class PageHeaderVM
    {
        public string Title { get; set; }
        public string Icon { get; set; }

        // Breadcrumb
        public string? ParentPage { get; set; } // Dashboard, Admin vs
        public string? ParentUrl { get; set; }

        // Badge / Count
        public int? Count { get; set; }
        public string? CountText { get; set; } // "ürün", "kategori"

        // Action button
        public string? CreateUrl { get; set; }
        public string? CreateText { get; set; }
    }
}
