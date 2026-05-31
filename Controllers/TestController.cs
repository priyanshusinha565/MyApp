using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    // Activity source for custom tracing
    private static readonly ActivitySource source = new ActivitySource("MyApp");

    [HttpGet]
    public async Task<IActionResult> Get()
    {
       // using var activity = source.StartActivity("Main Operation");

        try
        {
            // ✅ Step 1: Validation
            //using (var step1 = source.StartActivity("Validate Data"))
            //{
               // step1?.SetTag("step.name", "validation");
                Thread.Sleep(1000);
            //}

            // ✅ Step 2: Business Logic
            //using (var step2 = source.StartActivity("Business Logic"))
            //{
            //    step2?.SetTag("step.name", "business_logic");
                Thread.Sleep(500);
            //}

            // ✅ Step 3: DB Call (DB tracing)
            //using (var step3 = source.StartActivity("Database Call"))
            //{
            //    step3?.SetTag("db.system", "sqlserver");
               using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=TestDatabase;Trusted_Connection=True;TrustServerCertificate=True;"))
                {
                    await conn.OpenAsync();

                    var cmd = new SqlCommand("SELECT TOP 1 * FROM employee", conn);
                    await cmd.ExecuteReaderAsync();
                }
                //using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=TestDatabase;Trusted_Connection=True;TrustServerCertificate=True;"))
                //{
                //    await conn.OpenAsync();

                //    var cmd = new SqlCommand("SELECT TOP 1 * FROM Users", conn);
                 //    await cmd.ExecuteReaderAsync(); // 🔥 traced automatically
                //}
                int a = 10;
           // }

            // ✅ Step 4: External API Call
            using (var step4 = source.StartActivity("External API Call"))
            {
                step4?.SetTag("http.url", "https://google.com");

                var client = new HttpClient();
                var response = await client.GetAsync("https://google.com"); // 🔥 traced

                step4?.SetTag("http.status_code", (int)response.StatusCode);
            }

            return Ok("Done");
        }
        catch (Exception ex)
        {
            // ✅ Error tagging (VERY IMPORTANT)
            //activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            //activity?.SetTag("error", true);
            //activity?.SetTag("exception.message", ex.Message);
            //activity?.SetTag("exception.stacktrace", ex.StackTrace);

            throw; // rethrow for middleware logging
        }
    }
}