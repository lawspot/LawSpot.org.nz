using System;
using System.Collections.Generic;
using System.Linq;
using Lawspot.Shared;

namespace Lawspot.Views.Browse
{
    public class SearchPageViewModel
    {
        public string Query { get; set; }
        public PagedListView<SearchResultViewModel> Hits { get; set; }
    }

    public class SearchResultViewModel
    {
        public string Uri { get; set; }
        public string Title { get; set; }
        public string HighlightsHtml { get; set; }
        public string CreatedOn { get; set; }
        public string AnswerCount { get; set; }
    }
}