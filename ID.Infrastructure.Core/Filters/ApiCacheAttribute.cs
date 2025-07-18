using ID.Infrastructure.Core;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ID.Infrastructure.Filters
{
    public class ApiCacheAttribute : ActionFilterAttribute
    {
        private string[] _updateMethodStartNames = { "update", "save", "delete", "add" };

        private readonly ICacheService _cacheService;
        private readonly bool _preLoad;
        private readonly bool _scoped = true;
        public bool _cacheReadEnabled = false;
        public bool _cacheUpdateEnabled = false;
        private bool _cacheTypeExists = false;
        private bool _cacheKeyExists = false;
        private string _requestMethod = "Get";
        private string _apiRequestMethod = "Get";
        private string _cacheKey;
        private Type _modelType;

        public ApiCacheAttribute(Type modelType, bool preLoad = false)
        {
            _cacheService = GeneralContext.GetService<ICacheService>();
            _modelType = modelType;
            _preLoad = preLoad;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                _requestMethod = context.HttpContext.Request.Method;
                _apiRequestMethod = context.HttpContext.Request.Headers["ApiRequestMethod"].FirstOrDefault() ?? _requestMethod ?? "GET";

                _cacheReadEnabled = HttpMethods.IsGet(_apiRequestMethod) || HttpMethods.IsPost(_apiRequestMethod);
                _cacheUpdateEnabled = HttpMethods.IsPut(_apiRequestMethod)
                    || HttpMethods.IsPatch(_apiRequestMethod) || HttpMethods.IsDelete(_apiRequestMethod)
                    || (HttpMethods.IsPost(_apiRequestMethod)
                        && _updateMethodStartNames.Any(x => ((ControllerActionDescriptor)context.ActionDescriptor).ActionName.ToLower().StartsWith(x)));

                if (!_cacheUpdateEnabled)
                {
                    _cacheKey = _cacheService.CreateCacheKey(context.HttpContext.Request.Path, _modelType.Name);
                    _cacheKeyExists = _cacheService.ContainsKey(_cacheKey);
                }

                _cacheTypeExists = _cacheService.ContainsKeyType(_modelType.Name);
            }
            catch (Exception ex)
            {
                GeneralContext.Logger.Error(ex.GetApiMessageInfo());
                _cacheKeyExists = false;
            }

            if ((_cacheReadEnabled && !_cacheKeyExists) || _cacheUpdateEnabled)
                await next();
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            try
            {
                if (_cacheReadEnabled && _cacheKeyExists)
                    context.HttpContext.Response.Headers
                        .Add("Cache-Result", $"{_cacheKey}={DateTime.Now.ToUniversalTime().ToString()}");
                
                if (_cacheUpdateEnabled)
                    context.HttpContext.Response.Headers
                               .Add("Cache-Updated", $"{_cacheKey}={DateTime.Now.ToUniversalTime().ToString()}");

                ResultExecutedContext resultContext = await next();

                if (resultContext.Exception == null)
                {
                    if (_cacheUpdateEnabled)
                    {
                        _cacheService.RemoveKeyType(_modelType.Name);
                        _cacheTypeExists = _cacheService.ContainsKeyType(_modelType.Name);
                        _cacheKey = $"${_modelType.Name}";
                    }
                    else if (_cacheReadEnabled && !_cacheKeyExists)
                    {
                        var data = ((ObjectResult)context.Result)?.Value;
                        var controllerName = context.RouteData.Values["controller"].ToString();
                        var actionName = context.RouteData.Values["action"].ToString();

                        if (data != null)
                        {
                            _cacheKey = _cacheService.CreateCacheKey(context.HttpContext.Request.Path, _modelType.Name);
                            await _cacheService.SetAsync(_cacheKey, new ApiCacheItem(data, _scoped, _preLoad));
                            //context.HttpContext.Items[context.HttpContext.Request.Path] = data;
                        }
                    }
                }
                else
                {
                    resultContext.ExceptionHandled = true;
                    resultContext.Canceled = true;
                }
            }
            catch (Exception ex)
            {
                GeneralContext.Logger.Error(ex.GetApiMessageInfo());
                _cacheTypeExists = false;
                try { _cacheService.RemoveKeyType(_modelType.Name); } catch (Exception) { }
            }

            if (_cacheTypeExists)
                await base.OnResultExecutionAsync(context, next);
        }

        public override async void OnResultExecuting(ResultExecutingContext context)
        {
            try
            {
                if (_cacheReadEnabled && !_cacheUpdateEnabled && _cacheKey != null)
                {
                    _cacheKeyExists = _cacheService.ContainsKey(_cacheKey);
                    if (_cacheKeyExists)
                    {
                        var actionResult = await _cacheService.GetAsync<ApiCacheItem>(_cacheKey);
                        var data = Util.ConvertByteArrayToObject(actionResult.Data);
                        context.Result = new ObjectResult(data) { StatusCode = (int)HttpStatusCode.Accepted };
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralContext.Logger.Error(ex.GetApiMessageInfo());
                _cacheKeyExists = false;

                try { _cacheService.RemoveKeyType(_modelType.Name); } catch (Exception) { }

            }

            base.OnResultExecuting(context);
        }
    }
}
