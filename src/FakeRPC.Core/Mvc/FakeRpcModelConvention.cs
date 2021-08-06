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
                action.Selectors.Add(CreateActionSelector(controller, action.ActionName));
                return;
            }

            foreach (var selector in action.Selectors)
            {
                var routePath = controller.ControllerType.GetServiceRoute(action.ActionName);
                var routeModel = new AttributeRouteModel(new RouteAttribute(routePath));
                selector.AttributeRouteModel = routeModel;
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
            var selectorModel = new SelectorModel();
            var routePath = controller.ControllerType.GetServiceRoute(actionName);
            selectorModel.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(routePath));
            selectorModel.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { Constants.FAKE_RPC_HTTP_METHOD }));
            return selectorModel;
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

        private string BuildRoute(string areaName, string serviceGroup, string serviceName, string actionName)
        {
            return $"{Constants.FAKE_RPC_ROUTE_PREFIX}/{areaName}/{serviceGroup.Replace(".","/")}/{serviceName}/{actionName}".Replace("//", "/");
        }
    }
}
