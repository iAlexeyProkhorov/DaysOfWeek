using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.DiscountRules.DaysOfWeek
{
    public partial class RouteProvider : IRouteProvider
	{
        public void RegisterRoutes(IRouteBuilder routes)
        {
            routes.MapRoute("Plugin.DiscountRules.DaysOfWeek.Configure",
                 "Plugins/DiscountRulesDaysOfWeek/Configure",
                 new { controller = "DiscountRulesDaysOfWeek", action = "Configure", area = "Admin" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
