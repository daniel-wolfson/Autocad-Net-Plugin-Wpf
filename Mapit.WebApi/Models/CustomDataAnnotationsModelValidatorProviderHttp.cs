using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http.Metadata;
using System.Web.Http.Validation;
using System.Web.Http.Validation.Providers;

namespace MapIt.WebApi
{
    public class CustomDataAnnotationsModelValidatorProviderHttp : DataAnnotationsModelValidatorProvider
    {
        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata,
            IEnumerable<ModelValidatorProvider> validatorProviders, IEnumerable<Attribute> attributes)
        {
            List<Attribute> updatedAttributeList = attributes.ToList();

            // add test attribute here, query database for real implementation
            if (metadata != null && metadata.PropertyName != null &&
                metadata.PropertyName.Equals("Url", StringComparison.CurrentCultureIgnoreCase))
            {
                StringLengthAttribute stringLength =
                    new StringLengthAttribute(10) {MinimumLength = 4};
                updatedAttributeList.Add(stringLength);
            }
            return base.GetValidators(metadata, validatorProviders, updatedAttributeList);
        }

    }
}