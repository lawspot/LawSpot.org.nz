using System;
using System.Collections.Generic;
using System.Linq;
using Lawspot.Backend;

namespace Lawspot.Views.Browse
{
    public interface ITopCategories
    {
        IEnumerable<CategoryViewModel> TopCategories1 { get; set; }
        IEnumerable<CategoryViewModel> TopCategories2 { get; set; }
    }

    public interface IRecentAnswers
    {
        IEnumerable<AnsweredQuestionViewModel> RecentAnswers { get; set; }
    }

    public interface IMostViewedQuestions
    {
        IEnumerable<QuestionViewModel> MostViewedQuestions { get; set; }
    }

    public class AnsweredQuestionViewModel
    {
        public string Uri { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string AnsweredBy { get; set; }
        public string AnsweredTime { get; set; }
        public bool Last { get; set; }
    }

    public class QuestionViewModel
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string AnswerCount { get; set; }
        public string ViewCount { get; set; }
    }

    public class CategoryViewModel
    {
        public string Uri { get; set; }
        public string Name { get; set; }
        public int QuestionCount { get; set; }
    }
}