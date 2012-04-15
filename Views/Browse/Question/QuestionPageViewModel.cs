using System;
using System.Collections.Generic;
using System.Linq;

namespace Lawspot.Views.Browse
{
    public class QuestionPageViewModel : ITopCategories
    {
        public string Title { get; set; }
        public string Details { get; set; }
        public int CategoryId { get; set; }
        public string CategoryUrl { get; set; }
        public string CategoryName { get; set; }
        public string CreationDate { get; set; }
        public int Views { get; set; }
        public IEnumerable<AnswerViewModel> Answers { get; set; }

        public IEnumerable<CategoryViewModel> TopCategories1 { get; set; }
        public IEnumerable<CategoryViewModel> TopCategories2 { get; set; }
    }

    public class AnswerViewModel
    {
        public string Details { get; set; }
    }
}