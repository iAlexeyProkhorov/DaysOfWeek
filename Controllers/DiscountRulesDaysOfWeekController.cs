using Nop.Core.Domain.Discounts;
using Nop.Plugin.DiscountRules.DaysOfWeek.Extentions;
using Nop.Plugin.DiscountRules.DaysOfWeek.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.DiscountRules.DaysOfWeek.Controllers
{
	[AuthorizeAdmin]
	[Area(AreaNames.Admin)]
	public class DiscountRulesDaysOfWeekController : BasePluginController
    {
        #region Fields

        private readonly IDiscountService _discountService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Constructor

        public DiscountRulesDaysOfWeekController(IDiscountService discountService,
            ISettingService settingService,
            IPermissionService permissionService)
        {
            this._discountService = discountService;
            this._settingService = settingService;
            this._permissionService = permissionService;
        }

        #endregion

        #region Methods

        public ActionResult Configure(int discountId, int? discountRequirementId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;
            if (discountRequirementId.HasValue)
            {
                discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId.Value);
                if (discountRequirement == null)
                    return Content("Failed to load rule.");
            }

            var selectedDaysOfWeek = _settingService.GetSettingByKey<string>(string.Format(DaysOfWeekSystemNames.PluginSettingName, discountRequirementId.GetValueOrDefault(0)));

            var model = new RequirementModel();

            model.RequirementId = discountRequirementId.GetValueOrDefault(0);
            model.DiscountId = discountId;
            model.SelectedDaysOfWeekId = selectedDaysOfWeek.ParseSeparatedNumbers();

            model.AvailableDaysOfWeek = DayOfWeek.Monday.ToSelectList(useLocalization: true).ToList();
            foreach(var day in model.AvailableDaysOfWeek)
            {
                int dayIndex = 0;
                if (int.TryParse(day.Value, out dayIndex))
                    day.Selected = model.SelectedDaysOfWeekId.Contains(dayIndex);
            }

            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("DiscountRulesDaysOfWeek{0}", discountRequirementId.GetValueOrDefault(0));

            return View("~/Plugins/DiscountRules.DaysOfWeek/Views/Configure.cshtml", model);
        }

        [HttpPost]
        //[AdminAntiForgery]
        public ActionResult Configure(int discountId, int? discountRequirementId, int[] daysOfWeekIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;
            if (discountRequirementId.HasValue)
                discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId.Value);

            string daysOfWeek = string.Join(",", daysOfWeekIds);

            if (discountRequirement != null)
            {
                //update existing rule
                _settingService.SetSetting(string.Format(DaysOfWeekSystemNames.PluginSettingName, discountRequirement.Id), daysOfWeek);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement
                {
                    DiscountRequirementRuleSystemName = "DiscountRules.DaysOfWeek"
                };
                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);

                _settingService.SetSetting(string.Format(DaysOfWeekSystemNames.PluginSettingName, discountRequirement.Id), daysOfWeek);
            }
            return Json(new { Result = true, NewRequirementId = discountRequirement.Id });
        }

        #endregion
    }
}