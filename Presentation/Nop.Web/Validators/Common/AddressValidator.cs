﻿using FluentValidation;
using Nop.Core.Domain.Common;
using Nop.Services.Localization;
using Nop.Web.Models.Common;

namespace Nop.Web.Validators.Common
{
	public class AddressValidator : AbstractValidator<AddressModel>
	{
		public AddressValidator(ILocalizationService localizationService, AddressSettings addressSettings)
		{
			RuleFor(x => x.FirstName)
				.NotEmpty()
				.WithMessage(localizationService.GetResource("Address.Fields.FirstName.Required"));
			RuleFor(x => x.Email)
				.NotEmpty()
				.WithMessage(localizationService.GetResource("Address.Fields.Email.Required"));
			RuleFor(x => x.Email)
				.EmailAddress()
				.WithMessage(localizationService.GetResource("Common.WrongEmail"));
			if (addressSettings.CountryEnabled)
			{
				RuleFor(x => x.CountryId)
					.NotNull()
					.WithMessage(localizationService.GetResource("Address.Fields.Country.Required"));
				RuleFor(x => x.CountryId)
					.NotEqual(0)
					.WithMessage(localizationService.GetResource("Address.Fields.Country.Required"));
			}

			if (addressSettings.CompanyRequired && addressSettings.CompanyEnabled)
			{
				RuleFor(x => x.Company).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.Company.Required"));
			}
			if (addressSettings.StreetAddressRequired && addressSettings.StreetAddressEnabled)
			{
				RuleFor(x => x.Address1).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.StreetAddress.Required"));
			}
			if (addressSettings.StreetAddress2Required && addressSettings.StreetAddress2Enabled)
			{
				RuleFor(x => x.Address2).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.StreetAddress2.Required"));
			}
			if (addressSettings.ZipPostalCodeRequired && addressSettings.ZipPostalCodeEnabled)
			{
				RuleFor(x => x.ZipPostalCode).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.ZipPostalCode.Required"));
			}
			if (addressSettings.CityRequired && addressSettings.CityEnabled)
			{
				RuleFor(x => x.City).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.City.Required"));
			}
			if (addressSettings.PhoneRequired && addressSettings.PhoneEnabled)
			{
				RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.Phone.Required"));
				RuleFor(x => x.PhoneNumber).Matches(@"^[0-9]{8,11}$").WithMessage(localizationService.GetResource("Account.Fields.Phone.IncorrectFormat"));
			}
			if (addressSettings.FaxRequired && addressSettings.FaxEnabled)
			{
				RuleFor(x => x.FaxNumber).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.Fax.Required"));
			}
		}
	}
}