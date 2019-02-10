using MailKit.Net.Smtp;
using MimeKit;

namespace IdentityMicroservice
{
    public class SMTPClient
    {
        private static object oMail;

        public static void SendConfirmationEmail(string email, string link)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("EMSTeam", "ergfdfgsgasdfasd@gmail.com"));
            message.To.Add(new MailboxAddress(email));
            message.Subject = "Confirm Email";

            var emailText = @" Please confirm your e-mail by clicking the link: "
                +  link 
                + @"
            
            Thank you.
               
            Studnet Team";

            
            message.Body = new TextPart("plain")
            {
                Text = emailText
            };

            using (var client = new SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect("smtp.gmail.com", 465, true);

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate("ergfdfgsgasdfasd@gmail.com", "mypassword.notsarcastic");

                client.Send(message);
                client.Disconnect(true);
            }
        }

        public static void SendResetPasswordEmail(string email, string link)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("EMSTeam", "ergfdfgsgasdfasd@gmail.com"));
            message.To.Add(new MailboxAddress(email));
            message.Subject = "Reset Password";

            var emailText = @"Please reset your password by clicking here: "
                + link
                + @"
            
            Thank you.
               
            Studnet Team";


            message.Body = new TextPart("plain")
            {
                Text = emailText
            };

            using (var client = new SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect("smtp.gmail.com", 465, true);

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate("ergfdfgsgasdfasd@gmail.com", "mypassword.notsarcastic");

                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
