namespace EventManagementSystem.ViewModels
{
    public class PaginationHelper
    {
        public static List<int> GetPageNumbers(int currentPage, int totalPages, int range = 3)
        {
            var pages = new List<int>();

            int startPage = Math.Max(1, currentPage - range);
            int endPage = Math.Min(totalPages, currentPage + range);

            if (startPage > 1)
            {
                pages.Add(1);
                if (startPage > 2)
                    pages.Add(-1); // Represents "..."
            }

            for (int i = startPage; i <= endPage; i++)
            {
                pages.Add(i);
            }

            if (endPage < totalPages)
            {
                if (endPage < totalPages - 1)
                    pages.Add(-1); // Represents "..."
                pages.Add(totalPages);
            }

            return pages;
        }
    }
}
