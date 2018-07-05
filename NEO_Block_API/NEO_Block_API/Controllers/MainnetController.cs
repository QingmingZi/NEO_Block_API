﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Mvc;
using NEO_Block_API.lib;
using NEO_Block_API.RPC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NEO_Block_API.Controllers
{
    [Route("api/[controller]")]
    public class MainnetController : Controller
    {
        //接口返回最大忍受时间，超过则记录日志
        int logExeTimeMax = 15;

        Api api = new Api("mainnet");
        private ILog log = LogManager.GetLogger(Startup.repository.Name, typeof(MainnetController));

        [HttpGet]
        public JsonResult Get(string @jsonrpc, string @method, string @params, long @id)
        {
            JsonRPCrequest req = null;
            DateTime start = DateTime.Now;

            try
            {
                req = new JsonRPCrequest
                {
                    jsonrpc = @jsonrpc,
                    method = @method,
                    @params = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(JArray.Parse(@params))),
                    id = @id
                };

                string ipAddr = Request.HttpContext.Connection.RemoteIpAddress.ToString();

                var result = Json(api.getRes(req, ipAddr));
                if (DateTime.Now.Subtract(start).TotalSeconds > logExeTimeMax)
                {
                    log.Info(logHelper.logInfoFormat(req, result, start));
                }
                return result;
            }
            catch (Exception e)
            {
                JsonPRCresponse_Error resE = new JsonPRCresponse_Error(0, -100, "Parameter Error", e.Message);

                var result = Json(resE);
                log.Error(logHelper.logInfoFormat(req, result, start));
                return Json(result);

            }
        }

        [HttpPost]
        public async Task<JsonResult> Post()
        {
            JsonRPCrequest req = null;
            DateTime start = DateTime.Now;

            try
            {
                var ctype = HttpContext.Request.ContentType;
                LitServer.FormData form = null;
                if (ctype == "application/x-www-form-urlencoded" ||
                         (ctype.IndexOf("multipart/form-data;") == 0))
                    {
                        form = await LitServer.FormData.FromRequest(HttpContext.Request);
                        var _jsonrpc = form.mapParams["jsonrpc"];
                        var _id = long.Parse(form.mapParams["id"]);
                        var _method = form.mapParams["method"];
                        var _strparams = form.mapParams["params"];
                        var _params = JArray.Parse(_strparams);
                        req = new JsonRPCrequest
                        {
                            jsonrpc = _jsonrpc,
                            method = _method,
                            @params = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(_params)),
                            id = _id
                        };
                    }
                    else// if (ctype == "application/json") 其他所有请求方式都这样取好了
                    {
                        var text = await LitServer.FormData.GetStringFromRequest(HttpContext.Request);
                        req = JsonConvert.DeserializeObject<JsonRPCrequest>(text);
                    }

                    string ipAddr = Request.HttpContext.Connection.RemoteIpAddress.ToString();

                    var result = Json(api.getRes(req, ipAddr));
                    if (DateTime.Now.Subtract(start).TotalSeconds > logExeTimeMax)
                    {
                        log.Info(logHelper.logInfoFormat(req, result, start));
                    }
                    return result;
            }
            catch (Exception e)
            {
                JsonPRCresponse_Error resE = new JsonPRCresponse_Error(0, -100, "Parameter Error", e.Message);

                var result = Json(resE);
                log.Error(logHelper.logInfoFormat(req, result, start));
                return Json(result);
            }
        }
    }
}