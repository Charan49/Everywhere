using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;

namespace Web
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class StringTrimAttribute : Attribute
    {
    }

    /// <summary>
    /// MVC model binder which trims string values decorated with the <see cref="StringTrimAttribute"/>.
    /// </summary>
    /// 
    
    public class StringTrimModelBinder : IModelBinder
    {
        /// <summary>
        /// Binds the model, applying trimming when required.
        /// </summary>
        public bool BindModel(HttpActionContext controllerContext, ModelBindingContext bindingContext)
        {
            // Get binding value (return null when not present)
            var propertyName = bindingContext.ModelName;
            var originalValueResult = bindingContext.ValueProvider.GetValue(propertyName);
            if (originalValueResult == null)
                return false;
            var boundValue = originalValueResult.AttemptedValue;

            // Trim when required
            if (!String.IsNullOrEmpty(boundValue))
            {
                // Check for trim attribute
                if (bindingContext.ModelMetadata.ContainerType != null)
                {
                    var property = bindingContext.ModelMetadata.ContainerType.GetProperties()
                        .FirstOrDefault(propertyInfo => propertyInfo.Name == bindingContext.ModelMetadata.PropertyName);
                    if (property != null && property.GetCustomAttributes(true)
                        .OfType<StringTrimAttribute>().Any())
                    {
                        // Trim when attribute set
                        boundValue = boundValue.Trim();
                    }
                }
            }

            // Register updated "attempted" value with the model state
            bindingContext.ModelState.SetModelValue(propertyName, new ValueProviderResult(
                originalValueResult.RawValue, boundValue, originalValueResult.Culture));

            // Return bound value
            return true ;
        }
    }
    
}