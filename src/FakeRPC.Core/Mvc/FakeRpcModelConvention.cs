using FakeRpc.Core;
using FakeRpc.Core.Mics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FakeRpc.Core.Mvc
{
    public class FakeRpcModelConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                var type = controller.ControllerType.AsType();
                type = type.GetInterfaces()[0];
                var fakeRpc = type.GetCustomAttribute<FakeRpcAttribute>();
                if (fakeRpc != null)
                {
                    ConfigureApiExplorer(controller);
                    ConfigureSelector(controller);
                    ConfigureParameters(controller);
                }
            }
        }

        private void ConfigureApiExplorer(ControllerModel controller)
        {
            if (string.IsNullOrEmpty(controller.ApiExplorer.GroupName))
                controller.ApiExplorer.GroupName = controller.ControllerName;

            if (controller.ApiExplorer.IsVisible == null)
                controller.ApiExplorer.IsVisible = true;

            controller.Actions.ToList().ForEach(action => ConfigureApiExplorer(action));
        }

        private void ConfigureApiExplorer(ActionModel action)
        {
            if (action.ApiExplorer.IsVisible == null)
                action.ApiExplorer.IsVisible = true;
        }

        private void ConfigureSelector(ControllerModel controller)
        {
            ResetSelectors(controller.Selectors);

            if (controller.Selectors.Any(selector => selector.AttributeRouteModel != null))
                return;

            controller.Actions.ToList().ForEach(action => ConfigureSelector("", controller, action));
        }

        private void ConfigureSelector(string areaName, ControllerModel controller, ActionModel action)
        {
            ResetSelectors(action.Selectors);

            if (!action.Selectors.Any())
            {
                var selector = CreateActionSelector(controller, action.ActionName);
                action.Selectors.Add(selector);
                return;
            }

            foreach (var selector in action.Selectors)
            {
                var routeUrl = controller.ControllerType.GetServiceRoute(action.ActionName);
                selector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(routeUrl));
                selector.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { Constants.FAKE_RPC_HTTP_METHOD }));
            }
        }

        private void ConfigureParameters(ControllerModel controller)
        {
            controller.Actions.ToList().ForEach(action => ConfigureActionParameters(action));
        }

        private void ConfigureActionParameters(ActionModel action)
        {
            foreach (var parameter in action.Parameters)
            {
                if (parameter.BindingInfo != null)
                    continue;

                if (IsFromBodyEnable(action, parameter))
                    parameter.BindingInfo = BindingInfo.GetBindingInfo(new[] { new FromBodyAttribute() });
            }
        }

        private SelectorModel CreateActionSelector(ControllerModel controller, string actionName)
        {
            var selector = new SelectorModel();
            var routeUrl = controller.ControllerType.GetServiceRoute(actionName);
            selector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(routeUrl));
            selector.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { Constants.FAKE_RPC_HTTP_METHOD }));
            return selector;
        }

        private bool IsFromBodyEnable(ActionModel action, ParameterModel parameter)
        {
            foreach (var selector in action.Selectors)
            {
                if (selector.ActionConstraints == null)
                    continue;

                var httpMethods = new string[] { Constants.FAKE_RPC_HTTP_METHOD };
                var actionConstraints = selector.ActionConstraints
                    .Select(ac => ac as HttpMethodActionConstraint)
                    .Where(ac => ac != null)
                    .SelectMany(ac => ac.HttpMethods).Distinct().ToList();
                if (actionConstraints.Any(ac => httpMethods.Contains(ac)))
                    return true;
            }

            return false;
        }

        private void ResetSelectors(IList<SelectorModel> selectors)
        {
            selectors.ToList().RemoveAll(selector => selector.AttributeRouteModel == null && 
                (selector.ActionConstraints == null || !selector.ActionConstraints.Any())
            );
        }
    }
}
