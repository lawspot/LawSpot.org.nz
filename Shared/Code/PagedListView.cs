using System;
using System.Collections.Generic;
using System.Linq;

namespace Lawspot.Shared
{

    /// <summary>
    /// Represents a pageable list.
    /// </summary>
    /// <typeparam name="T"> The type of item in the list. </typeparam>
    public class PagedListView<T>
    {
        /// <summary>
        /// Constructs a new PagedListView.
        /// </summary>
        /// <param name="list"> The full list of items. </param>
        /// <param name="pageNum"> The current page number. </param>
        /// <param name="pageSize"> The size of each page. </param>
        /// <param name="requestUri"> The URI of the current page. </param>
        public PagedListView(IEnumerable<T> list, int pageNum, int pageSize, Uri requestUri)
        {
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException("pageSize");
            this.PageSize = pageSize;
            this.TotalCount = list.Count();
            this.PageNumber = Math.Max(Math.Min(pageNum, this.PageCount), 1);
            this.Items = list.Skip((this.PageNumber - 1) * this.PageSize).Take(this.PageSize).ToList();
            this.HasPrevious = this.PageNumber > 1;
            if (this.HasPrevious)
                this.PreviousUri = StringUtilities.SetUriParameter(requestUri, "page", this.PageNumber - 1);
            this.HasNext = this.PageNumber < this.PageCount;
            if (this.HasNext)
                this.NextUri = StringUtilities.SetUriParameter(requestUri, "page", this.PageNumber + 1);

            // There are always 7 page numbers visible (unless there are less than that many pages
            // available).
            // Examples:
            // 1 2 3 [4] 5 6
            // 1 2 3 4 5 [6]
            // [1] 2 3 4 5 6 .. 12
            // 1 2 3 [4] 5 6 .. 12
            // 1 .. 4 5 [6] 7 8 .. 12
            // 1 .. 7 8 [9] 10 11 12
            // 1 .. 7 8 9 10 11 [12]
            var labels = new List<PageLabel>();
            if (this.PageCount > 1)
            {
                int labelsBeforeCurrent = Math.Max(Math.Min(this.PageNumber - 1, 6) - Math.Min(this.PageCount - this.PageNumber, Math.Max(this.PageNumber - 4, 0)), 0);
                int labelsAfterCurrent = Math.Max(Math.Min(this.PageCount - this.PageNumber, 6) - Math.Min(this.PageNumber - 1, Math.Max(this.PageCount - this.PageNumber - 3, 0)), 0);
                if (this.PageNumber > labelsBeforeCurrent + 1)
                {
                    // 1 .. 4 5
                    labels.Add(PageLabel.CreateClickableLabel(1, requestUri));
                    labels.Add(PageLabel.CreateEllipsis());
                    for (int i = this.PageNumber - labelsBeforeCurrent + 1; i < this.PageNumber; i++)
                        labels.Add(PageLabel.CreateClickableLabel(i, requestUri));
                }
                else
                {
                    // 1 2 3
                    for (int i = this.PageNumber - labelsBeforeCurrent; i < this.PageNumber; i++)
                        labels.Add(PageLabel.CreateClickableLabel(i, requestUri));
                }
                labels.Add(PageLabel.CreateCurrentPageLabel(this.PageNumber));
                if (this.PageNumber < this.PageCount - labelsAfterCurrent)
                {
                    // 7 8 .. 12
                    for (int i = this.PageNumber + 1; i < this.PageNumber + labelsAfterCurrent; i++)
                        labels.Add(PageLabel.CreateClickableLabel(i, requestUri));
                    labels.Add(PageLabel.CreateEllipsis());
                    labels.Add(PageLabel.CreateClickableLabel(this.PageCount, requestUri));
                }
                else
                {
                    // 5 6 7
                    for (int i = this.PageNumber + 1; i <= this.PageNumber + labelsAfterCurrent; i++)
                        labels.Add(PageLabel.CreateClickableLabel(i, requestUri));
                }
            }
            this.PageLabels = labels;
        }

        /// <summary>
        /// The number of records in each page.
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// The total number of records.
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// The current page number, starting from 1.
        /// </summary>
        public int PageNumber { get; private set; }

        /// <summary>
        /// The total number of pages.
        /// </summary>
        public int PageCount
        {
            get { return (this.TotalCount + this.PageSize - 1) / this.PageSize; }
        }

        /// <summary>
        /// The items in the current page.
        /// </summary>
        public IList<T> Items { get; private set; }

        /// <summary>
        /// Indicates whether page labels should be shown.
        /// </summary>
        public bool ShowPageLabels
        {
            get { return this.PageCount > 1; }
        }

        /// <summary>
        /// Indicates whether the current page can be moved backward one page.
        /// </summary>
        public bool HasPrevious { get; private set; }

        /// <summary>
        /// The URI of the previous page.
        /// </summary>
        public Uri PreviousUri { get; private set; }

        /// <summary>
        /// Indicates whether the current page can be moved forward one page.
        /// </summary>
        public bool HasNext { get; private set; }

        /// <summary>
        /// The URI of the next page.
        /// </summary>
        public Uri NextUri { get; private set; }

        /// <summary>
        /// An enumerable list of page labels, suitable to make clickable buttons out of.
        /// </summary>
        public IEnumerable<PageLabel> PageLabels { get; private set; }
    }

    /// <summary>
    /// Represents a page label.
    /// </summary>
    public class PageLabel
    {
        /// <summary>
        /// Creates an clickable page label.
        /// </summary>
        /// <returns> A clickable page label. </returns>
        public static PageLabel CreateClickableLabel(int pageNumber, Uri requestUri)
        {
            return new PageLabel() { Text = pageNumber.ToString(), NavigateUri = StringUtilities.SetUriParameter(requestUri, "page", pageNumber) };
        }

        /// <summary>
        /// Creates a page label for the current page.
        /// </summary>
        /// <returns> A page label for the current page. </returns>
        public static PageLabel CreateCurrentPageLabel(int pageNumber)
        {
            return new PageLabel() { Text = pageNumber.ToString(), IsCurrentPage = true };
        }

        /// <summary>
        /// Creates an ellipsis page label.
        /// </summary>
        /// <returns> An ellipsis page label. </returns>
        public static PageLabel CreateEllipsis()
        {
            return new PageLabel() { Text = "…", IsEllipsis = true };
        }

        /// <summary>
        /// The text that should be displayed in the label.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Indicates whether this label is an ellipsis.
        /// </summary>
        public bool IsEllipsis { get; private set; }

        /// <summary>
        /// Indicates whether this label is for the current page.
        /// </summary>
        public bool IsCurrentPage { get; private set; }

        /// <summary>
        /// Indicates whether the label can be clicked.
        /// </summary>
        public bool IsClickable
        {
            get { return this.NavigateUri != null; }
        }

        /// <summary>
        /// The URI to navigate to if the label is clicked.  Will be <c>null</c> for labels that
        /// should not be clickable.
        /// </summary>
        public Uri NavigateUri { get; private set; }
    }

}