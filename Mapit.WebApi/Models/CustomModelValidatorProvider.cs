using System.Collections.Generic;
using System.Web.Http.Metadata;
using System.Web.Http.Validation;

namespace MapIt.WebApi
{
    public class CustomModelValidatorProvider : ModelValidatorProvider

    {
        public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata,
            IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            // create list to return
            List<ModelValidator> listModelValidators = new List<ModelValidator>();

            // create list of validator providers with our custom Data Annotations Model Validator Provider
            List<ModelValidatorProvider> listModelValidatorProviders = new List<ModelValidatorProvider>();
            listModelValidatorProviders.Add(new CustomDataAnnotationsModelValidatorProviderHttp());

            listModelValidators.Add(ModelValidator.GetModelValidator(listModelValidatorProviders));

            return listModelValidators;
        }

    }
}