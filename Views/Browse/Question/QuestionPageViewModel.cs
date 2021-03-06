﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Lawspot.Views.Browse
{
    public class QuestionPageViewModel : ITopCategories, IMostViewedQuestions
    {
        public int QuestionId { get; set; }
        public string Title { get; set; }
        public string DetailsHtml { get; set; }
        public int CategoryId { get; set; }
        public string CategoryUrl { get; set; }
        public string CategoryName { get; set; }
        public int Views { get; set; }
        public IEnumerable<AnswerViewModel> Answers { get; set; }

        public IEnumerable<CategoryViewModel> TopCategories1 { get; set; }
        public IEnumerable<CategoryViewModel> TopCategories2 { get; set; }
        public IEnumerable<QuestionViewModel> MostViewedQuestions { get; set; }
    }

    public class AnswerViewModel
    {
        public string DetailsHtml { get; set; }
        public string PublishedDate { get; set; }
        public string PublisherName { get; set; }
    }
}