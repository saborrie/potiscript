using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using PotiScript.Exceptions;
using PotiScript.Grammar;


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace PotiScript.Runtime
{
    public class ASTVisitor : AST.NodeVisitor<ReferenceSystem.Reference>
    {

        private readonly ReferenceSystem.Scope globalScope;
        private ReferenceSystem.Scope scope;
        private bool mustReturn;

        public ASTVisitor(ReferenceSystem.Scope globalScope)
        {
            this.globalScope = globalScope;
            this.scope = globalScope;
        }


        public async Task<ReferenceSystem.Reference> Visit(AST.NumericLiteral node, CancellationToken ct)
        {
            return new ReferenceSystem.ValueWrapper(new TypeSystem.Number(node.Value));
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.StringLiteral node, CancellationToken ct)
        {
            return new ReferenceSystem.ValueWrapper(new TypeSystem.String(node.Value));
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.BooleanLiteral node, CancellationToken ct)
        {
            return new ReferenceSystem.ValueWrapper(new TypeSystem.Boolean(node.Value));
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.NullLiteral node, CancellationToken ct)
        {
            return new ReferenceSystem.ValueWrapper(new TypeSystem.Null());
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.Identifier node, CancellationToken ct)
        {
            return new ReferenceSystem.Identifier(node.Value);
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.ExpressionStatement node, CancellationToken ct)
        {
            return await node.Expression.Accept(this, ct);
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.BlockStatement node, CancellationToken ct)
        {
            scope = scope.Push();

            ReferenceSystem.Reference? result = null;

            foreach (var innerStatement in node.Body)
            {
                result = await innerStatement.Accept(this, ct);

                if (mustReturn) break;
            }
            if (result != null)
            {
                result = new ReferenceSystem.ValueWrapper(result.ReadOrConvertToVariable(scope));
            }

            scope = scope.Pop();

            return result ?? new ReferenceSystem.ValueWrapper(new TypeSystem.Null());
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.ReturnStatement node, CancellationToken ct)
        {
            ReferenceSystem.Reference? result = null;
            if (node.Argument != null)
            {
                result = await node.Argument.Accept(this, ct);
            }

            mustReturn = true;
            return result ?? new ReferenceSystem.ValueWrapper(new TypeSystem.Null());
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.VariableStatement node, CancellationToken ct)
        {
            ReferenceSystem.Reference? result = null;

            foreach (var declaration in node.Declarations)
            {
                await declaration.Accept(this, ct);
            }

            return result ?? new ReferenceSystem.ValueWrapper(new TypeSystem.Null());
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.IfStatement node, CancellationToken ct)
        {
            ReferenceSystem.Reference result;
            scope = scope.Push();

            if ((await node.Test.Accept(this, ct)).Truthy(scope))
            {
                result = await node.Consequent.Accept(this, ct);
            }
            else if (node.Alternate != null)
            {
                result = await node.Alternate.Accept(this, ct);
            }
            else
            {
                result = new ReferenceSystem.ValueWrapper(new TypeSystem.Null());
            }

            scope = scope.Pop();
            return result;
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.WhileStatement node, CancellationToken ct)
        {
            ReferenceSystem.Reference? result = null;
            scope = scope.Push();

            while ((await node.Test.Accept(this, ct)).Truthy(scope))
            {
                result = await node.Body.Accept(this, ct);
            }
            scope = scope.Pop();

            return result ?? new ReferenceSystem.ValueWrapper(new TypeSystem.Null());
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.DoWhileStatement node, CancellationToken ct)
        {
            ReferenceSystem.Reference result;

            scope = scope.Push();

            do
            {
                result = await node.Body.Accept(this, ct);
            }
            while ((await node.Test.Accept(this, ct)).Truthy(scope));

            scope = scope.Pop();

            return result;
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.ForStatement node, CancellationToken ct)
        {
            ReferenceSystem.Reference? result = null;

            scope = scope.Push();

            if (node.Init != null)
            {
                await node.Init!.Accept(this, ct);
            }

            async Task<bool> test()
            {
                if (node.Test != null)
                {
                    return (await node.Test.Accept(this, ct)).Truthy(scope);
                }
                return true;
            }

            async Task update()
            {
                if (node.Update != null)
                {
                    await node.Update.Accept(this, ct);
                }
            }

            for (; await test(); await update())
            {
                result = await node.Body.Accept(this, ct);
            }

            scope = scope.Pop();

            return result ?? new ReferenceSystem.ValueWrapper(new TypeSystem.Null());
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.ForeachStatement node, CancellationToken ct)
        {
            ReferenceSystem.Reference? result = null;

            var idReference = (await node.Id.Accept(this, ct)).FullName;
            var enumeratorReference = (await node.Enumerator.Accept(this, ct));

            var enumerator = enumeratorReference.ReadOrConvertToVariable(scope);

            if (!enumerator.GetType().IsAssignableTo(typeof(IAsyncEnumerable<TypeSystem.Object>)))
            {
                throw new TypeErrorException($"{enumeratorReference.FullName} is not enumerable.");
            }

            await foreach (var item in (enumerator as IAsyncEnumerable<TypeSystem.Object>)!)
            {
                scope = scope.Push();

                scope.Declare(idReference);
                scope[idReference] = item;

                result = await node.Body.Accept(this, ct);

                scope = scope.Pop();
            }

            return result ?? new ReferenceSystem.ValueWrapper(new TypeSystem.Null());
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.EmptyStatement node, CancellationToken ct)
        {
            return new ReferenceSystem.ValueWrapper(new TypeSystem.Null());
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.FunctionDeclaration node, CancellationToken ct)
        {
            var reference = (await node.Name.Accept(this, ct)).FullName;


            var parameterTasks = node.Parameters!.Select(x => x.Accept(this, ct));
            var parameters = (await Task.WhenAll(parameterTasks)).Select(x => x.FullName).ToArray();

            var functionScope = scope;

            var function = new TypeSystem.Function(async (args, ct) =>
            {
                scope = functionScope.Push();

                for (var i = 0; i < parameters!.Length; i++)
                {
                    scope.Declare(parameters[i]);
                    scope[parameters[i]] = args[i];
                }

                return (await node.Body.Accept(this, ct)).ReadOrConvertToVariable(scope);
            });

            scope.Declare(reference);
            scope[reference] = function;
            return new ReferenceSystem.ValueWrapper(function);
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.VariableDeclaration node, CancellationToken ct)
        {
            var variable = (await node.Id.Accept(this, ct)).ToVariable(scope);

            scope.Declare(variable.Name);
            if (node.Init != null)
            {
                variable.Write((await node.Init.Accept(this, ct)).Read());
            }
            return variable;
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.AssignmentExpression node, CancellationToken ct)
        {
            var lhs = await node.Left.Accept(this, ct);
            var oldValue = lhs.ReadOrConvertToVariable(scope);
            var right = (await node.Right.Accept(this, ct)).ReadOrConvertToVariable(scope);

            var newValue = node.Operator switch
            {
                "=" => right,
                "+=" => oldValue.BinaryOperation("+", right),
                "-=" => oldValue.BinaryOperation("-", right),
                "*=" => oldValue.BinaryOperation("*", right),
                "/=" => oldValue.BinaryOperation("/", right),
                _ => throw new InvalidOperationException(@$"Unknown operator ""{node.Operator}""")
            };

            lhs.WriteOrConvertToVariable(scope, newValue);

            return lhs;
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.BinaryExpression node, CancellationToken ct)
        {
            var left = (await node.Left.Accept(this, ct)).ReadOrConvertToVariable(scope);
            var right = (await node.Right.Accept(this, ct)).ReadOrConvertToVariable(scope);

            return new ReferenceSystem.ValueWrapper(left.BinaryOperation(node.Operator, right));
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.LogicalExpression node, CancellationToken ct)
        {
            var left = (await node.Left.Accept(this, ct)).ReadOrConvertToVariable(scope);
            var right = (await node.Right.Accept(this, ct)).ReadOrConvertToVariable(scope);

            return new ReferenceSystem.ValueWrapper(left.BinaryOperation(node.Operator, right));
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.UnaryExpression node, CancellationToken ct)
        {
            var operand = (await node.Operand.Accept(this, ct)).ReadOrConvertToVariable(scope);

            return new ReferenceSystem.ValueWrapper(operand.UnaryOperation(node.Operator));
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.MemberExpression node, CancellationToken ct)
        {
            var parent = new ReferenceSystem.ValueWrapper((await node.Object.Accept(this, ct)).ReadOrConvertToVariable(scope));

            if (parent.Value.IsNull() && node.NullPropagation)
            {
                return parent;
            }

            if (node.Computed)
            {
                var key = (await node.Property.Accept(this, ct)).ReadOrConvertToVariable(scope).ConvertToString();
                return new ReferenceSystem.Member(parent, key);
            }

            return (await node.Property.Accept(this, ct)).ToMember(parent);
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.CallExpression node, CancellationToken ct)
        {
            var callee = await node.Callee.Accept(this, ct);

            var argsTasks = node.Arguments.Select(argument => argument.Accept(this, ct));
            var args = (await Task.WhenAll(argsTasks)).Select(argument => argument.ReadOrConvertToVariable(scope)).ToArray();

            var function = callee.ReadOrConvertToVariable(scope) as TypeSystem.Function;

            if (function == null)
            {
                throw new TypeErrorException(@$"""{callee!.FullName}"" is not a function");
            }

            var returnScope = scope;
            var result = await function.Call(args, ct);
            mustReturn = false;
            scope = returnScope;
            return new ReferenceSystem.ValueWrapper(result);
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.Program node, CancellationToken ct)
        {
            this.scope = globalScope;
            this.mustReturn = false;

            ReferenceSystem.Reference result = new ReferenceSystem.ValueWrapper(new TypeSystem.Null());

            foreach (var innerStatement in node.Body)
            {
                result = await innerStatement.Accept(this, ct);

                if (mustReturn) break;
            }

            return result;
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.TemplateLiteral node, CancellationToken ct)
        {
            var builder = new StringBuilder();

            foreach (var segment in node.Segments)
            {
                builder.Append((await segment.Accept(this, ct)).ReadOrConvertToVariable(scope).ConvertToString());
            }

            return new ReferenceSystem.ValueWrapper(new TypeSystem.String(builder.ToString()));
        }

        public async Task<ReferenceSystem.Reference> Visit(AST.PartialString node, CancellationToken ct)
        {
            return new ReferenceSystem.ValueWrapper(new TypeSystem.String(node.Value));
        }
    }
}
