using System.Linq.Expressions;

namespace System.Reflection
{
    [TestClass]
    public partial class ExpressionTest
    {
        [TestMethod]
        public void Test()
        {
            var _instance = Expression.Parameter(typeof(MyClass), "instance");

            var property = Expression.Property(_instance, "BB");
            var convert = Expression.Convert(property, typeof(object));
            var getter = Expression.Lambda<Func<MyClass, object>>(convert, _instance).Compile();
            var val = getter(new MyClass { AA = "123" });
            Assert.IsTrue(val == null);
        }

        //https://expressiontree-tutorial.net/dynamic-linq-expression
        [TestMethod("Dynamic LINQ Expression")]
        public void Test2()
        {
            var customers = new List<string>();

            // The IQueryable data to query.  
            IQueryable<String> queryableData = customers.AsQueryable<string>();

            // Compose the expression tree that represents the parameter to the predicate.  
            ParameterExpression pe = Expression.Parameter(typeof(string), "customer");

            var t = typeof(MyClass);
            
            // Create an expression tree that represents the expression 'customer.ToLower() == "diego roel"'.  
            Expression left = Expression.Call(pe, typeof(string).GetMethod("ToLower",System.Type.EmptyTypes));
            Expression right = Expression.Constant("diego roel");
            Expression e1 = Expression.Equal(left, right);

            // Create an expression tree that represents the expression 'customer.Length <= 12'.  
            left = Expression.Property(pe, typeof(string).GetProperty("Length"));
            right = Expression.Constant(12, typeof(int));
            Expression e2 = Expression.LessThanOrEqual(left, right);

            // Combine the expression trees to create an expression tree that represents the  
            // expression '(customer.ToLower() == "diego roel" || customer.Length <= 12)'.  
            Expression predicateBody = Expression.OrElse(e1, e2);

            // Create an expression tree that represents the expression  
            // 'customers.Where(customer => (customer.ToLower() == "diego roel" || customer.Length <= 12))'  
            MethodCallExpression whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { queryableData.ElementType },
                queryableData.Expression,
                Expression.Lambda<Func<string, bool>>(predicateBody, new ParameterExpression[] { pe }));
            // ***** End Where *****  

            // ***** OrderBy(customer => customer) *****  
            // Create an expression tree that represents the expression  
            // 'whereCallExpression.OrderBy(customer => customer)'  
            MethodCallExpression orderByCallExpression = Expression.Call(
                typeof(Queryable),
                "OrderBy",
                new Type[] { queryableData.ElementType, queryableData.ElementType },
                whereCallExpression,
                Expression.Lambda<Func<string, string>>(pe, new ParameterExpression[] { pe }));
            // ***** End OrderBy *****  

            // Create an executable query from the expression tree.  
            IQueryable<string> results = queryableData.Provider.CreateQuery<string>(orderByCallExpression);
        }

        //https://expressiontree-tutorial.net/method-access-expression
        [TestMethod("Method Access Expression")]
        public void Test3()
        {
            string text = "This is simple text.";
            Expression callExpr = Expression.Call(
            Expression.Constant(text), typeof(String).GetMethod("ToUpper", new Type[] { }));

            // The following statement first creates an expression tree,
            // then compiles it and then executes it.  
            Console.WriteLine(Expression.Lambda<Func<String>>(callExpr).Compile()());

            int param1 = 1;
            int param2 = 2;
            var c1 = Expression.Constant(param1);
            var c2 = Expression.Constant(param2);
            var expr = Expression.Call(typeof(MyClass).GetMethod("MyFunc"), c1, c2);
            Func<int> func = Expression.Lambda<Func<int>>(expr).Compile();

            Console.WriteLine(func.Invoke());
        }
    }
}
