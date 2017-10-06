using EmailSevice.Implementation;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSevice.Action
{
    public class SendEmailAction : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationService _service = factory.CreateOrganizationService(context.UserId);

            string response = string.Empty;
            Guid contactId = Guid.Empty;

            if (context.InputParameters.Contains("ContactId") && context.InputParameters["ContactId"].ToString() != string.Empty &&
                Guid.TryParse(context.InputParameters["ContactId"].ToString(), out contactId))
            {
                EmailSender emailSender = new EmailSender(_service);
                response = emailSender.SendEmail(contactId, context.UserId);
            }            
            context.OutputParameters["SendEmailResponse"] = response;
        }
    }
}

