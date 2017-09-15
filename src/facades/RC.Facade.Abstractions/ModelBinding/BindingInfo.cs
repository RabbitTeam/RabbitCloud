using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Facade.Abstractions.ModelBinding
{
    public class BindingInfo
    {
        public BindingSource BindingSource { get; set; }
        public string BinderModelName { get; set; }
        public Type BinderType { get; set; }
        public object DefaultValue { get; set; }

        public Func<ServiceRequestContext, bool> RequestPredicate { get; set; }

        public static BindingInfo GetBindingInfo(IReadOnlyCollection<object> attributes)
        {
            var bindingInfo = new BindingInfo();
            var isBindingInfoPresent = false;

            // BinderModelName
            foreach (var binderModelNameAttribute in attributes.OfType<IModelNameProvider>())
            {
                isBindingInfoPresent = true;
                if (binderModelNameAttribute?.Name != null)
                {
                    bindingInfo.BinderModelName = binderModelNameAttribute.Name;
                    break;
                }
            }

            // BinderType
            foreach (var binderTypeAttribute in attributes.OfType<IBinderTypeProviderMetadata>())
            {
                isBindingInfoPresent = true;
                if (binderTypeAttribute.BinderType != null)
                {
                    bindingInfo.BinderType = binderTypeAttribute.BinderType;
                    break;
                }
            }

            // BindingSource
            foreach (var bindingSourceAttribute in attributes.OfType<IBindingSourceMetadata>())
            {
                isBindingInfoPresent = true;
                if (bindingSourceAttribute.BindingSource != null)
                {
                    bindingInfo.BindingSource = bindingSourceAttribute.BindingSource;
                    break;
                }
            }

            // RequestPredicate
            foreach (var requestPredicateProvider in attributes.OfType<IRequestPredicateProvider>())
            {
                isBindingInfoPresent = true;
                if (requestPredicateProvider.RequestPredicate != null)
                {
                    bindingInfo.RequestPredicate = requestPredicateProvider.RequestPredicate;
                    break;
                }
            }

            // DefaultValue
            foreach (var defaultValueProvider in attributes.OfType<IDefaultValueProviderMetadata>())
            {
                isBindingInfoPresent = true;
                if (defaultValueProvider.Value != null)
                {
                    bindingInfo.DefaultValue = defaultValueProvider.Value;
                    break;
                }
            }

            return isBindingInfoPresent ? bindingInfo : null;
        }
    }
}