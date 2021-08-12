using System;
using System.ComponentModel.DataAnnotations;
using CJG.Core.Entities;
using CJG.Web.External.Areas.Ext.Models.OrganizationProfile;

namespace CJG.Web.External.Areas.Ext.Models
{
    public class OrganizationProfileViewModelValidation
    {
        public static ValidationResult ValidateYearEstablished(int? YearEstablished, ValidationContext context)
        {
            var model = context.ObjectInstance as OrganizationProfileViewNewModel;
            if (model == null)
	            throw new ArgumentNullException();
            var result = ValidationResult.Success;

            if (!YearEstablished.HasValue || YearEstablished < 1800 || YearEstablished > AppDateTime.UtcNow.Year)
	            result = new ValidationResult($"Year established must be between 1800 and {AppDateTime.UtcNow.Year}");

            return result;
        }

        public static ValidationResult ValidateNumberOfEmployeesInBC(int? NumberOfEmployeesInBC, ValidationContext context)
        {
            var model = context.ObjectInstance as OrganizationProfileViewNewModel;
            if (model == null)
	            throw new ArgumentNullException();

            var result = ValidationResult.Success;

            if (!NumberOfEmployeesInBC.HasValue || NumberOfEmployeesInBC > model.NumberOfEmployeesWorldwide)
	            result = new ValidationResult("Number of employees in BC can't be greater than number of employees worldwide");

            return result;
        } 
    }

}