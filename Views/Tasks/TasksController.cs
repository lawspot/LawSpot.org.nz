using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Lawspot.Backend;
using Lawspot.Views.Admin;

namespace Lawspot.Controllers
{
    public class TasksController : BaseController
    {
        /// <summary>
        /// Sends a reminder email to all registered lawyers.
        /// </summary>
        [HttpGet]
        public void SendReminderEmails()
        {
            // Get a list of all the questions that have no answers or even draft answers.
            var questions = this.DataContext.Questions.Where(q => q.Approved && q.Answers.Any() == false && q.DraftAnswers.Any() == false).ToList();

            int sentMessageCount = 0;
            if (questions.Count > 0)
            {
                // Compose messages to all the lawyers.
                var messages = new List<Email.LawyerReminderMessage>();
                foreach (var lawyer in this.DataContext.Users.Where(l => l.ApprovedAsLawyer.HasValue && l.ApprovedAsLawyer.Value == true))
                    messages.Add(new Email.LawyerReminderMessage(lawyer, questions));

                // Send the messages.
                foreach (var message in messages)
                {
                    try
                    {
                        message.Send();
                    }
                    catch (Exception ex)
                    {
                        Lawspot.Shared.Logger.LogError(ex, "Could not send lawyer reminder email to {0}", message.To);
                    }
                }

                sentMessageCount = messages.Count;
            }
        }
    }
}
