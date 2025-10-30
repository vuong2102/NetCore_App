namespace Net_Learning
{
    public class MyCustomMiddleware
    {
        private readonly RequestDelegate _next;

        public MyCustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            return context.Response.WriteAsync("Hello from MyCustomMiddleware!");
            //return _next(context);
        }
    }
}
