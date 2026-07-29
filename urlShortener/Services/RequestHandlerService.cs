using Microsoft.AspNetCore.Mvc;

namespace urlShortener.Services
{
    public class RequestHandlerService
    {
        public async Task<IActionResult> HandleRequest<T>(Func<Task<T>> action)
        {
            try
            {
                var result = await action();
                return new OkObjectResult(result);
            }
            catch (InvalidOperationException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new ObjectResult("Internal Server Error")
                {
                    StatusCode = 500
                };
            }
        }


        public async Task<IActionResult> HandleRequest(Func<Task> action)
        {
            try
            {
                await action();
                return new OkResult();
            }
            catch (InvalidOperationException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new ObjectResult("Internal Server Error")
                {
                    StatusCode = 500
                };
            }
        }
    }
}