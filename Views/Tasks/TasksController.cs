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
        /// Sends a test message.
        /// </summary>
        [HttpGet]
        public void Test()
        {
            var message = new System.Net.Mail.MailMessage();
            message.To.Add("paulbartrum@hotmail.com");
            message.Body = "Tasks are indeed working.  Jolly good show!";
            message.IsBodyHtml = false;

            // Send the email.
            using (var client = new System.Net.Mail.SmtpClient())
            {
                client.Send(message);
            }
        }

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
                foreach (var lawyer in this.DataContext.Lawyers.Where(l => l.Approved))
                    messages.Add(new Email.LawyerReminderMessage(lawyer, questions));

                // Send the messages.
                foreach (var message in messages)
                    message.Send();

                sentMessageCount = messages.Count;
            }
        }
    }
}
