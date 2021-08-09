using FakeRpc.Core;
using MessagePack;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FakeRpc.Example.Interface
{
    public class CalculatorService : ICalculatorService
    {
        public Task<CalculatorReply> Calculate(CalculatorRequest request)
        {
            var exp = string.Empty;
            var result = 0M;
            switch (request.Op)
            {
                case "+":
                    exp = $"{request.Num1} + {request.Num2} =";
                    result = request.Num1 + request.Num2;
                    break;
                case "-":
                    exp = $"{request.Num1} - {request.Num2} =";
                    result = request.Num1 - request.Num2;
                    break;
                case "*":
                    exp = $"{request.Num1} * {request.Num2} =";
                    result = request.Num1 * request.Num2;
                    break;
                case "/":
                    exp = $"{request.Num1} / {request.Num2} = ";
                    result = request.Num1 / request.Num2;
                    break;
            }

            return Task.FromResult(new CalculatorReply() { Expression = exp, Result = result });
        }

        public Task<CalculatorReply> Random()
        {
            var operators = new string[] { "+", "-", "*", "/" };
            var random = new Random();
            var num1 = random.Next(0, 100);
            var num2 = random.Next(1, 100);
            var op = operators[random.Next(operators.Length)];
            return Calculate(new CalculatorRequest() { Num1 = num1, Num2 = num2, Op = op });
        }
    }
}
