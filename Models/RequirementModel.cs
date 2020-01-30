using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.DiscountRules.DaysOfWeek.Models
{
	public class RequirementModel
    {
        public RequirementModel()
        {
            this.AvailableDaysOfWeek = new List<SelectListItem>();
            this.SelectedDaysOfWeekId = new List<int>();
        }
        
        [NopResourceDisplayName("Plugins.DiscountRules.DaysOfWeek.Fields.SelectedDaysOfWeekId")]
        public IList<int> SelectedDaysOfWeekId { get; set; }

        public IList<SelectListItem> AvailableDaysOfWeek { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }
    }
}