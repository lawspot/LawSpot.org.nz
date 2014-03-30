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
        public LawyerReminderMessage(User user, IEnumerable<Question> unansweredQuestions)
        {
            this.To.Add(user.EmailDisplayName);
            this.TemplateFilePath = "LawyerReminder.xslt";
            this.Subject = "Unanswered questions posted on LawSpot";
            this.Name = user.FirstName;

            if (user.SpecialisationCategoryId.HasValue)
            {
                this.SpecialtyName = user.SpecialisationCategory.Name;
                this.SpecialtyUnansweredQuestionCount = unansweredQuestions.Count(q => q.CategoryId == user.SpecialisationCategoryId);
                this.SpecialtyUnansweredQuestions = unansweredQuestions.Where(q => q.CategoryId == user.SpecialisationCategoryId).OrderByDescending(q => q.CreatedOn)
                    .Select(q => new UnansweredQuestion()
                    {
                        Title = q.Title,
                        Uri = q.Uri,
                    });
                unansweredQuestions = unansweredQuestions.Where(q => q.CategoryId != user.SpecialisationCategoryId);
            }

            this.UnansweredQuestionCount = unansweredQuestions.Count();
            this.UnansweredQuestions = unansweredQuestions.OrderByDescending(q => q.CreatedOn)
                .Select(q => new UnansweredQuestion()
                {
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
        /// The number of unanswered questions (not including specialty questions).
        /// </summary>
        [ExposeToXslt]
        private int UnansweredQuestionCount { get; set; }

        /// <summary>
        /// The number of unanswered questions in the lawyer's specialty area.
        /// </summary>
        [ExposeToXslt]
        private int SpecialtyUnansweredQuestionCount { get; set; }

        /// <summary>
        /// The name of the specialty area.
        /// </summary>
        [ExposeToXslt]
        private string SpecialtyName { get; set; }

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

        /// <summary>
        /// The unanswered questions in the lawyer's specialty area.
        /// </summary>
        [ExposeToXslt]
        private IEnumerable<UnansweredQuestion> SpecialtyUnansweredQuestions { get; set; }
    }

}