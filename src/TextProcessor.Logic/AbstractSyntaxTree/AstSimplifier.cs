namespace TextProcessor.Logic.AbstractSyntaxTree;

/// <summary>
/// Алгебраический и логический упрощатель для AST.
/// </summary>
public static class AstSimplifier
{
    public static AstExpression Simplify(AstExpression expr) => expr switch
    {
        Num or Var => expr,
        Neg neg => SimplifyNeg(Simplify(neg.AstExpression)),
        Abs abs => SimplifyAbs(Simplify(abs.AstExpression)),
        Bin bin => SimplifyBin(bin.Operator, Simplify(bin.First), Simplify(bin.Second)),
        _ => expr
    };

    public static AstPredication Simplify(AstPredication pred) => pred switch
    {
        BoolLiteral => pred,
        Not not => SimplifyNot(Simplify(not.Predication)),
        Cmp cmp => SimplifyCmp(cmp.Operator, Simplify(cmp.First), Simplify(cmp.Second)),
        And and => SimplifyAnd(Simplify(and.First), Simplify(and.Second)),
        Or or => SimplifyOr(Simplify(or.First), Simplify(or.Second)),
        _ => pred
    };

    private static AstExpression SimplifyNeg(AstExpression inner) => inner switch
    {
        Num num => new Num(-num.Value),
        Neg neg => neg.AstExpression,
        _ => new Neg(inner)
    };

    private static AstExpression SimplifyAbs(AstExpression inner) => inner switch
    {
        Num num => new Num(Math.Abs(num.Value)),
        Neg neg => new Abs(neg.AstExpression),
        _ => new Abs(inner)
    };

    private static AstExpression SimplifyBin(ExpressionOperatorKind op, AstExpression left, AstExpression right)
    {
        // Свёртка числовых констант
        if (left is Num n1 && right is Num n2)
        {
            return op switch
            {
                ExpressionOperatorKind.Sum => new Num(n1.Value + n2.Value),
                ExpressionOperatorKind.Multiply => new Num(n1.Value * n2.Value),
                ExpressionOperatorKind.Divide when n2.Value != 0 && n1.Value % n2.Value == 0 => new Num(n1.Value / n2.Value),
                _ => new Bin(op, left, right)
            };
        }

        return op switch
        {
            ExpressionOperatorKind.Sum => SimplifySum(left, right),
            ExpressionOperatorKind.Multiply => SimplifyMultiply(left, right),
            ExpressionOperatorKind.Divide => SimplifyDivide(left, right),
            _ => new Bin(op, left, right)
        };
    }

    private static AstExpression SimplifySum(AstExpression left, AstExpression right)
    {
        // Нейтральный элемент: x + 0 = x, 0 + x = x
        if (right is Num { Value: 0 }) return left;
        if (left is Num { Value: 0 }) return right;

        // Взаимоуничтожение: x + (-x) = 0
        if (right is Neg negR && left.Equals(negR.AstExpression)) return new Num(0);
        if (left is Neg negL && negL.AstExpression.Equals(right)) return new Num(0);

        // Свёртка ассоциативных констант: (x + c1) + c2 -> x + (c1 + c2)
        if (left is Bin { Operator: ExpressionOperatorKind.Sum, First: var e1, Second: Num c1 } && right is Num c2)
            return Simplify(new Bin(ExpressionOperatorKind.Sum, e1, new Num(c1.Value + c2.Value)));

        if (left is Bin { Operator: ExpressionOperatorKind.Sum, First: Num c1b, Second: var e2 } && right is Num c2b)
            return Simplify(new Bin(ExpressionOperatorKind.Sum, e2, new Num(c1b.Value + c2b.Value)));

        if (left is Num c1c && right is Bin { Operator: ExpressionOperatorKind.Sum, First: var e3, Second: Num c2c })
            return Simplify(new Bin(ExpressionOperatorKind.Sum, e3, new Num(c1c.Value + c2c.Value)));

        if (left is Num c1d && right is Bin { Operator: ExpressionOperatorKind.Sum, First: Num c2d, Second: var e4 })
            return Simplify(new Bin(ExpressionOperatorKind.Sum, e4, new Num(c1d.Value + c2d.Value)));

        return new Bin(ExpressionOperatorKind.Sum, left, right);
    }

    private static AstExpression SimplifyMultiply(AstExpression left, AstExpression right)
    {
        if (left is Num { Value: 0 } || right is Num { Value: 0 }) return new Num(0);
        if (right is Num { Value: 1 }) return left;
        if (left is Num { Value: 1 }) return right;
        if (right is Num { Value: -1 }) return SimplifyNeg(left);
        if (left is Num { Value: -1 }) return SimplifyNeg(right);

        return new Bin(ExpressionOperatorKind.Multiply, left, right);
    }

    private static AstExpression SimplifyDivide(AstExpression left, AstExpression right)
    {
        if (left is Num { Value: 0 }) return new Num(0);
        if (right is Num { Value: 1 }) return left;
        if (left.Equals(right)) return new Num(1);

        return new Bin(ExpressionOperatorKind.Divide, left, right);
    }

    private static AstPredication SimplifyNot(AstPredication inner) => inner switch
    {
        BoolLiteral b => new BoolLiteral(!b.Value),
        Not n => n.Predication,
        Cmp c => new Cmp(InvertOperator(c.Operator), c.First, c.Second),
        _ => new Not(inner)
    };

    private static AstPredication SimplifyCmp(PredicationOperatorKind op, AstExpression left, AstExpression right)
    {
        // Сравнение двух чисел
        if (left is Num n1 && right is Num n2)
        {
            bool val = op switch
            {
                PredicationOperatorKind.Equal => n1.Value == n2.Value,
                PredicationOperatorKind.NotEqual => n1.Value != n2.Value,
                PredicationOperatorKind.Less => n1.Value < n2.Value,
                PredicationOperatorKind.LessOrEqual => n1.Value <= n2.Value,
                PredicationOperatorKind.Greater => n1.Value > n2.Value,
                PredicationOperatorKind.GreaterOrEqual => n1.Value >= n2.Value,
                _ => false
            };
            return new BoolLiteral(val);
        }

        // Сравнение идентичных выражений: A == A -> true, A < A -> false
        if (left.Equals(right))
        {
            bool isReflexive = op is PredicationOperatorKind.Equal
                or PredicationOperatorKind.LessOrEqual
                or PredicationOperatorKind.GreaterOrEqual;
            return new BoolLiteral(isReflexive);
        }

        // Перенос слагаемого: expr + c1 <op> c2  ==>  expr <op> c2 - c1
        if (left is Bin { Operator: ExpressionOperatorKind.Sum, First: var expr1, Second: Num c1 } && right is Num c2)
            return SimplifyCmp(op, expr1, new Num(c2.Value - c1.Value));

        if (left is Bin { Operator: ExpressionOperatorKind.Sum, First: Num c1b, Second: var expr2 } && right is Num c2b)
            return SimplifyCmp(op, expr2, new Num(c2b.Value - c1b.Value));

        // Деление на положительный множитель: k * expr <op> c  ==>  expr <op> c / k (при k > 0 и нацело)
        if (left is Bin { Operator: ExpressionOperatorKind.Multiply, First: Num { Value: > 0 } k1, Second: var mExpr1 }
            && right is Num mc1 && mc1.Value % k1.Value == 0)
        {
            return SimplifyCmp(op, mExpr1, new Num(mc1.Value / k1.Value));
        }

        if (left is Bin { Operator: ExpressionOperatorKind.Multiply, First: var mExpr2, Second: Num { Value: > 0 } k2 }
            && right is Num mc2 && mc2.Value % k2.Value == 0)
        {
            return SimplifyCmp(op, mExpr2, new Num(mc2.Value / k2.Value));
        }

        return new Cmp(op, left, right);
    }

    private static AstPredication SimplifyAnd(AstPredication left, AstPredication right)
    {
        if (left is BoolLiteral { Value: false } || right is BoolLiteral { Value: false })
            return new BoolLiteral(false);
        if (left is BoolLiteral { Value: true }) return right;
        if (right is BoolLiteral { Value: true }) return left;
        if (left.Equals(right)) return left;

        return new And(left, right);
    }

    private static AstPredication SimplifyOr(AstPredication left, AstPredication right)
    {
        if (left is BoolLiteral { Value: true } || right is BoolLiteral { Value: true })
            return new BoolLiteral(true);
        if (left is BoolLiteral { Value: false }) return right;
        if (right is BoolLiteral { Value: false }) return left;
        if (left.Equals(right)) return left;

        return new Or(left, right);
    }

    private static PredicationOperatorKind InvertOperator(PredicationOperatorKind op) => op switch
    {
        PredicationOperatorKind.Equal => PredicationOperatorKind.NotEqual,
        PredicationOperatorKind.NotEqual => PredicationOperatorKind.Equal,
        PredicationOperatorKind.Less => PredicationOperatorKind.GreaterOrEqual,
        PredicationOperatorKind.LessOrEqual => PredicationOperatorKind.Greater,
        PredicationOperatorKind.Greater => PredicationOperatorKind.LessOrEqual,
        PredicationOperatorKind.GreaterOrEqual => PredicationOperatorKind.Less,
        _ => op
    };
}