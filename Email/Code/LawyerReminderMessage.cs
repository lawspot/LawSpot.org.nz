using System;
using System.Collections.Generic;
using System.Linq;
using Lawspot.Backend;

namespace Lawspot.Email
{

    /// <summary>
    /// The email sent to lawyers to remind them to answer questions.
    /// </summary>
    public class LawyerReminderMessage : EmailTemplate
    {
        public LawyerReminderMessage(Lawyer lawyer, IEnumerable<Question> unansweredQuestions)
        {
            this.To.Add(lawyer.User.EmailDisplayName);
            this.TemplateFilePath = "LawyerReminder.xslt";
            this.Subject = "Unanswered questions posted on LawSpot";
            this.Name = lawyer.FirstName;
            this.UnansweredQuestionCount = unansweredQuestions.Count();
            this.UnansweredQuestions = unansweredQuestions.OrderBy(q => q.CategoryId == lawyer.SpecialisationCategoryId ? 0 : 1)
                .ThenByDescending(q => q.CreatedOn)
                .Select(q => new UnansweredQuestion() {
                Title = q.Title,
                Uri = q.Uri,
            });
        }

        /// <summary>
        /// The name of the lawyer.
        /// </summary>
        [ExposeToXslt]
        private string Name { get; set; }

        /// <summary>
        /// The number of unanswered questions.
        /// </summary>
        [ExposeToXslt]
        private int UnansweredQuestionCount { get; set; }

        // Helper class.
        private class UnansweredQuestion
        {
            [ExposeToXslt]
            public string Title { get; set; }

            [ExposeToXslt]
            public string Uri { get; set; }
        }

        /// <summary>
        /// The unanswered questions.
        /// </summary>
        [ExposeToXslt]
        private IEnumerable<UnansweredQuestion> UnansweredQuestions { get; set; }
    }

}