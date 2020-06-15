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
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Plugin.DiscountRules.DaysOfWeek.Extentions;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using System;

namespace Nop.Plugin.DiscountRules.DaysOfWeek
{
    public partial class DaysOfWeekDiscountRequirementRule : BaseBaroquePlugin, IDiscountRequirementRule
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        private readonly ISettingService _settingService;

        private readonly IWebHelper _webHelper;

        public DaysOfWeekDiscountRequirementRule(IActionContextAccessor actionContextAccessor,
            IUrlHelperFactory urlHelperFactory, 
            ISettingService settingService,
            IWebHelper webHelper)
        {
            this._actionContextAccessor = actionContextAccessor;
            this._urlHelperFactory = urlHelperFactory;

            this._settingService = settingService;
            this._webHelper = webHelper;
        }

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>Result</returns>
        public DiscountRequirementValidationResult CheckRequirement(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            //invalid by default
            var result = new DiscountRequirementValidationResult();

            if (request.Customer == null)
                return result;

            var selectedDayIds = _settingService.GetSettingByKey<string>(string.Format(DaysOfWeekSystemNames.PluginSettingName, request.DiscountRequirementId)).ParseSeparatedNumbers();

            if (selectedDayIds.Contains((int)DateTime.UtcNow.DayOfWeek))
                result.IsValid = true;

            return result;
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            //configured in RouteProvider.cs
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "DiscountRulesDaysOfWeek",
                new { discountId = discountId, discountRequirementId = discountRequirementId }, _webHelper.CurrentRequestProtocol);
        }

        public override void Install()
        {
            base.Install();
        }

        public override void Uninstall()
        {
            base.Uninstall();
        }
    }
}