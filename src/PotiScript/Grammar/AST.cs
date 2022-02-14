using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PotiScript.Grammar
{
    public static class AST
    {
        public interface NodeVisitor<T>
        {
            Task<T> Visit(NumericLiteral node, CancellationToken ct);
            Task<T> Visit(StringLiteral node, CancellationToken ct);
            Task<T> Visit(BooleanLiteral node, CancellationToken ct);
            Task<T> Visit(NullLiteral node, CancellationToken ct);
            Task<T> Visit(Identifier node, CancellationToken ct);
            Task<T> Visit(ExpressionStatement node, CancellationToken ct);
            Task<T> Visit(BlockStatement node, CancellationToken ct);
            Task<T> Visit(ReturnStatement node, CancellationToken ct);
            Task<T> Visit(VariableStatement node, CancellationToken ct);
            Task<T> Visit(IfStatement node, CancellationToken ct);
            Task<T> Visit(WhileStatement node, CancellationToken ct);
            Task<T> Visit(DoWhileStatement node, CancellationToken ct);
            Task<T> Visit(ForStatement node, CancellationToken ct);
            Task<T> Visit(ForeachStatement node, CancellationToken ct);
            Task<T> Visit(EmptyStatement node, CancellationToken ct);
            Task<T> Visit(FunctionDeclaration node, CancellationToken ct);
            Task<T> Visit(VariableDeclaration node, CancellationToken ct);
            Task<T> Visit(AssignmentExpression node, CancellationToken ct);
            Task<T> Visit(TemplateLiteral node, CancellationToken ct);
            Task<T> Visit(PartialString node, CancellationToken ct);
            Task<T> Visit(BinaryExpression node, CancellationToken ct);
            Task<T> Visit(LogicalExpression node, CancellationToken ct);
            Task<T> Visit(UnaryExpression node, CancellationToken ct);
            Task<T> Visit(MemberExpression node, CancellationToken ct);
            Task<T> Visit(CallExpression node, CancellationToken ct);
            Task<T> Visit(Program node, CancellationToken ct);
        }

        public abstract record Node(string Type)
        {
            public virtual Task<T> Accept<T>(NodeVisitor<T> visitor, CancellationToken ct)
            {
                if (ct.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                return visitor.Visit((dynamic)this, ct);
            }
        };

        public class NodeList : List<Node>
        { }

        // literals
        public record NumericLiteral(string Token, decimal Value) : Node(nameof(NumericLiteral));
        public record StringLiteral(string Value) : Node(nameof(StringLiteral));
        public record BooleanLiteral(bool Value) : Node(nameof(BooleanLiteral));
        public record NullLiteral() : Node(nameof(NullLiteral));
        public record TemplateLiteral(NodeList Segments) : Node(nameof(TemplateLiteral));
        public record PartialString(string Value) : Node(nameof(PartialString));


        // identifier
        public record Identifier(string Value) : Node(nameof(Identifier));

        // statements
        public record ExpressionStatement(Node Expression) : Node(nameof(ExpressionStatement));
        public record BlockStatement(NodeList Body) : Node(nameof(BlockStatement));
        public record ReturnStatement(Node? Argument) : Node(nameof(ReturnStatement));
        public record VariableStatement(NodeList Declarations) : Node(nameof(VariableStatement));
        public record IfStatement(Node Test, Node Consequent, Node? Alternate = null) : Node(nameof(IfStatement));
        public record WhileStatement(Node Test, Node Body) : Node(nameof(WhileStatement));
        public record DoWhileStatement(Node Body, Node Test) : Node(nameof(DoWhileStatement));
        public record ForStatement(Node? Init, Node? Test, Node? Update, Node Body) : Node(nameof(ForStatement));
        public record ForeachStatement(Node Id, Node Enumerator, Node Body) : Node(nameof(ForStatement));
        public record EmptyStatement() : Node(nameof(EmptyStatement));
        public record FunctionDeclaration(Node Name, NodeList Parameters, Node Body) : Node(nameof(FunctionDeclaration));
        public record VariableDeclaration(Node Id, Node? Init = null) : Node(nameof(VariableDeclaration));

        // expressions
        public record AssignmentExpression(string Operator, Node Left, Node Right) : Node(nameof(AssignmentExpression));
        public record BinaryExpression(string Operator, Node Left, Node Right) : Node(nameof(BinaryExpression));
        public record LogicalExpression(string Operator, Node Left, Node Right) : Node(nameof(LogicalExpression));
        public record UnaryExpression(string Operator, Node Operand) : Node(nameof(UnaryExpression));
        public record MemberExpression(bool NullPropagation, bool Computed, Node Object, Node Property) : Node(nameof(MemberExpression));
        public record CallExpression(Node Callee, NodeList Arguments) : Node(nameof(CallExpression));

        // program
        public record Program(NodeList Body) : Node(nameof(Program));
    }
}
