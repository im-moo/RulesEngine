// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace DemoApp
{
    public class JSONDemo
    {
        public void Run()
        {
            Console.WriteLine($"Running {nameof(JSONDemo)}....");
            var basicInfo = "{\"name\": \"hello\",\"email\": \"abcy@xyz.com\",\"creditHistory\": \"good\",\"country\": \"canada\",\"loyaltyFactor\": 3,\"totalPurchasesToDate\": 10000}";
            var orderInfo = "{\"totalOrders\": 5,\"recurringItems\": 2}";
            var telemetryInfo = "{\"noOfVisitsPerMonth\": 10,\"percentageOfBuyingToVisit\": 15}";
            //定义规则
            var rulesStr = @"[
  {
    ""WorkflowName"": ""UserInputWorkflow"",
    ""Rules"": [
      {
        ""RuleName"": ""test1"",
        ""ErrorMessage"": ""error test1"",
        ""Enabled"": true,
        ""ErrorType"": ""Warning"",
        ""Expression"": ""Age > 18 Or IdNo != null Or Name != null"",
        ""SuccessEvent"": ""test1""
      },
{
        ""RuleName"": ""test6"",
        ""ErrorMessage"": ""error test6"",
        ""Enabled"": true,
        ""ErrorType"": ""Warning"",
        ""Expression"": ""Age > 18 Or IdNo != null And Name != null"",
        ""SuccessEvent"": ""test6""
      },
{
        ""RuleName"": ""test7"",
        ""ErrorMessage"": ""error test7"",
        ""Enabled"": true,
        ""ErrorType"": ""Warning"",
        ""Expression"": ""Age > 18 AndAlso (IdNo != null Or Name != null)"",
        ""SuccessEvent"": ""test7""
      },
{
        ""RuleName"": ""test2"",
        ""ErrorMessage"": ""error test2"",
        ""Enabled"": true,
        ""ErrorType"": ""Warning"",
        ""Expression"": ""Age > 18 And IdNo != null And Name != null"",
        ""SuccessEvent"": ""test2""
      },
{
        ""RuleName"": ""test3"",
        ""ErrorMessage"": ""error test3"",
        ""Enabled"": true,
        ""ErrorType"": ""Warning"",
        ""Expression"": ""Age > 18 "",
        ""SuccessEvent"": ""test3""
      }
,
{
        ""RuleName"": ""test4"",
        ""ErrorMessage"": ""error test4"",
        ""Enabled"": true,
        ""ErrorType"": ""Warning"",
        ""Expression"": ""IdNo != null"",
        ""SuccessEvent"": ""test4""
      }
,
{
        ""RuleName"": ""test5"",
        ""ErrorMessage"": ""error test5"",
        ""Enabled"": true,
        ""ErrorType"": ""Warning"",
        ""Expression"": ""Name != null"",
        ""SuccessEvent"": ""test5""
      }
    ]
  }
]";
            var converter = new ExpandoObjectConverter();

            dynamic input1 = JsonConvert.DeserializeObject<ExpandoObject>(basicInfo, converter);
            dynamic input2 = JsonConvert.DeserializeObject<ExpandoObject>(orderInfo, converter);
            dynamic input3 = JsonConvert.DeserializeObject<ExpandoObject>(telemetryInfo, converter);

            var inputs = new dynamic[]
                {
                    input1,
                    input2,
                    input3
                };

            var userInput = new UserInput {
                IdNo = "2020",
                Age = 18
            };

            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Discount.json", SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
                throw new Exception("Rules not found.");

            var fileData = File.ReadAllText(files[0]);
            var workflow = JsonConvert.DeserializeObject<List<Workflow>>(fileData);

            var bre = new RulesEngine.RulesEngine(workflow.ToArray(), null);
            //反序列化Json格式规则字符串
            var workflowRules = JsonConvert.DeserializeObject<List<Workflow>>(rulesStr);
            var rulesEngine = new RulesEngine.RulesEngine(workflowRules.ToArray());
            List<RuleResultTree> resultLists = rulesEngine.ExecuteAllRulesAsync("UserInputWorkflow", userInput).Result;
            foreach (var item in resultLists)
            {
                Console.WriteLine("验证结果：{0}，消息：{1}，规则名：{2}，规则：{3}", item.IsSuccess ? "命中" : "失败", item.ExceptionMessage, item.Rule.RuleName, item.Rule.Expression);
            }
            resultLists.OnSuccess((eventName) => {
                var msg = $"至少命中了一条 {eventName} 老子成功了.";
                Console.WriteLine(msg);
            }).OnFail(() => {
                Console.WriteLine("老子全失败了！！！");
            });
            Console.ReadLine();
            //以上是实验
            string discountOffered = "No discount offered.";

            List<RuleResultTree> resultList = bre.ExecuteAllRulesAsync("Discount", inputs).Result;
            foreach (var ruleResult in resultList)
            {
                Console.WriteLine("验证成功：{0}，消息：{1}", ruleResult.IsSuccess, ruleResult.ExceptionMessage);
            }
            resultList.OnSuccess((eventName) => {
                discountOffered = $"Discount offered is {eventName} % over MRP.";
            });

            resultList.OnFail(() => {
                discountOffered = "The user is not eligible for any discount." + resultList.Select(x => x.IsSuccess == false).Count();
            });

            Console.WriteLine(discountOffered);
        }

        public class UserInput
        {
            public string IdNo { get; set; }
            public int Age { get; set; }
            public string Name { get; set; }
        }
    }
}
