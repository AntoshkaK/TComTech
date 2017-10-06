using EmailSevice.Implementation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSevice.Plugin
{
    public class EmailCounterPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationService _service = factory.CreateOrganizationService(context.UserId);

            if (context.MessageName == "Update" && context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity target = (Entity)context.InputParameters["Target"];
                if (target.Contains("statecode") && target.GetAttributeValue<OptionSetValue>("statecode").Value == 1)
                {
                    EmailSender emailSender = new EmailSender(_service);
                    emailSender.UpdatLetterCountForUser(target.Id);
                }
            }
        }
    }
}