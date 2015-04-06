using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;

namespace EDR.Utilities.Validators
{
    public class RequiredStringArrayValueAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                Type type = value.GetType();
                PropertyInfo[] pinfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                PropertyInfo strProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.PropertyType == typeof(string[])).FirstOrDefault();

                foreach (string val in (string[])strProperties.GetValue(value))
                {
                    var isChecked = val != null && val != "";
                    if (isChecked)
                    {
                        return ValidationResult.Success;
                    }
                }
            }

            return new ValidationResult(base.ErrorMessageString);
        }
    }
}