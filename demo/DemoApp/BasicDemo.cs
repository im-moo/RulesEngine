// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using Newtonsoft.Json;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace DemoApp
{
    public class BasicDemo
    {
        public void Run()
        {
            Console.WriteLine($"Running {nameof(BasicDemo)}....");
            List<Workflow> workflows = new List<Workflow>();
            Workflow workflow = new Workflow();
            workflow.WorkflowName = "Test Workflow Rule 1";

            List<Rule> rules = new List<Rule>();
            List<Rule> rules1 = new List<Rule>();

            Rule rule = new Rule();
            rule.RuleName = "Test Rule";
            rule.SuccessEvent = "Count is within tolerance.";
            rule.ErrorMessage = "Over expected.";
            //rule.Expression = "count < 3";
            rule.RuleExpressionType = RuleExpressionType.LambdaExpression;
            rule.Operator = "And";

            Rule rule2 = new Rule();
            rule2.RuleName = "Test Rule2";
            rule2.SuccessEvent = "Count is within tolerance.";
            rule2.ErrorMessage = "Over expected.";
            rule2.Expression = "count < 3";
            rule2.RuleExpressionType = RuleExpressionType.LambdaExpression;

            Rule rule3 = new Rule();
            rule3.RuleName = "Test Rule3";
            rule3.SuccessEvent = "Count is within tolerance.";
            rule3.ErrorMessage = "Over expected.";
            rule3.Expression = "count > 3";
            rule3.RuleExpressionType = RuleExpressionType.LambdaExpression;

            rule.Rules = rules;
            rules.Add(rule2);
            rules.Add(rule3);
            rules1.Add(rule);
            workflow.Rules = rules1;

            workflows.Add(workflow);

            var input1 = JsonConvert.SerializeObject(workflows);

            var bre = new RulesEngine.RulesEngine(workflows.ToArray(), null);

            dynamic datas = new ExpandoObject();
            datas.count = 1;
            var inputs = new dynamic[]
              {
                    datas
              };

            List<RuleResultTree> resultList = bre.ExecuteAllRulesAsync("Test Workflow Rule 1", inputs).Result;

            bool outcome = false;

            //Different ways to show test results:
            outcome = resultList.TrueForAll(r => r.IsSuccess);
            Console.WriteLine($"Test outcome1: {outcome}.");
            resultList.OnSuccess((eventName) => {
                Console.WriteLine($"Result '{eventName}' is as expected.");
                outcome = true;
            });
            Console.WriteLine($"Test outcome2: {outcome}.");
            resultList.OnFail(() => {
                outcome = false;
                Console.WriteLine($"Test OnFail: {!outcome}.");
            });

            Console.WriteLine($"Test outcome3: {outcome}.");
        }
    }
}
