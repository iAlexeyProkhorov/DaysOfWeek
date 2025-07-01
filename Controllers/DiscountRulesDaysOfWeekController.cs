//Copyright 2020 Alexey Prokhorov

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.DiscountRules.DaysOfWeek.Extentions;
using Nop.Plugin.DiscountRules.DaysOfWeek.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.DiscountRules.DaysOfWeek.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    public class DiscountRulesDaysOfWeekController : BasePluginController
    {
        #region Fields

        private readonly IDiscountService _discountService;
        private readonly ISettingService _settingService;

        #endregion

        #region Constructor

        public DiscountRulesDaysOfWeekController(IDiscountService discountService,
            ISettingService settingService)
        {
            this._discountService = discountService;
            this._settingService = settingService;
        }

        #endregion

        #region Methods

        [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
        public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
        {
            var discount = _discountService.GetDiscountByIdAsync(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.GetValueOrDefault(0));
            if (discountRequirementId.HasValue && discountRequirement == null)
                return Content("Failed to load rule.");

            var selectedDaysOfWeek = await _settingService.GetSettingByKeyAsync<string>(string.Format(DaysOfWeekSystemNames.PluginSettingName, discountRequirementId.GetValueOrDefault(0)));

            var model = new RequirementModel();

            model.RequirementId = discountRequirementId.GetValueOrDefault(0);
            model.DiscountId = discountId;
            model.SelectedDaysOfWeekId = selectedDaysOfWeek.ParseSeparatedNumbers();

            model.AvailableDaysOfWeek = (await DayOfWeek.Monday.ToSelectListAsync(useLocalization: true)).ToList();
            foreach (var day in model.AvailableDaysOfWeek)
            {
                int dayIndex = 0;
                if (int.TryParse(day.Value, out dayIndex))
                    day.Selected = model.SelectedDaysOfWeekId.Contains(dayIndex);
            }

            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("DiscountRulesDaysOfWeek{0}", discountRequirementId.GetValueOrDefault(0));

            return View("~/Plugins/DiscountRules.DaysOfWeek/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(int discountId, int? discountRequirementId, int[] daysOfWeekIds)
        {
            var discount = await _discountService.GetDiscountByIdAsync(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.GetValueOrDefault(0));
            var daysOfWeek = string.Join(",", daysOfWeekIds);

            if (discountRequirement != null)
                //update existing rule
                await _settingService.SetSettingAsync(string.Format(DaysOfWeekSystemNames.PluginSettingName, discountRequirement.Id), daysOfWeek);
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement
                {
                    DiscountRequirementRuleSystemName = "DiscountRules.DaysOfWeek",
                    DiscountId = discountId
                };
                await _discountService.InsertDiscountRequirementAsync(discountRequirement);

                await _settingService.SetSettingAsync(string.Format(DaysOfWeekSystemNames.PluginSettingName, discountRequirement.Id), daysOfWeek);
            }
            return Json(new { Result = true, NewRequirementId = discountRequirement.Id });
        }

        #endregion
    }
}