using Xunit;
using static SpecRec.GlobalObjectFactory;

namespace SpecRec.Tests
{
    /// <summary>
    /// Integration tests demonstrating the intended usage of the [SpecRec] attribute.
    /// These tests show exactly how the unified SpecRec interface should work in practice.
    /// </summary>
    public class SpecRecExecutorIntegrationTests
    {
        /// <summary>
        /// Basic substitute pattern - demonstrates auto-parrot creation via ctx.Substitute()
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task BookFlight(Context ctx, int passengerCount, string airlineCode = "UA")
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                ctx.Substitute<IBookingRepository>("💾")
                   .Substitute<IFlightService>("✈️");

                var coordinator = new BookingCoordinator();
                return coordinator.BookFlight(passengerCount, airlineCode);
            }, ctx);
        }

        /// <summary>
        /// Object registration pattern - mix of real objects and parrots
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task ProcessPayment(Context ctx, decimal amount, string currency = "USD")
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                var paymentProcessor = new PaymentProcessorStub();
                var logger = new FakeLogger();
                
                ctx.SetAlways<IPaymentProcessor>(paymentProcessor, "mainProcessor")
                   .SetOne<ILogger>(logger, "logger1")
                   .Substitute<IAuditService>("📊");

                var service = new EnhancedPaymentService();
                return service.ProcessPayment(amount, currency);
            }, ctx);
        }

        /// <summary>
        /// CallLogger wrapping pattern - track existing object method calls
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task TrackExternalCalls(Context ctx, string endpoint, int retryCount = 3)
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                var apiClient = new HttpApiClientStub();
                var trackedClient = ctx.Wrap<IHttpApiClient>(apiClient, "🔗");

                var service = new ExternalService(trackedClient);
                return service.FetchDataWithRetries(endpoint, retryCount);
            }, ctx);
        }

        /// <summary>
        /// Direct parrot creation pattern - create parrot without registering with ObjectFactory
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task ValidateInput(Context ctx, string input, bool strictMode = false)
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                var validator = ctx.Parrot<IValidator>("✅");
                
                var service = new ValidationService(validator);
                return service.ValidateUserInput(input, strictMode);
            }, ctx);
        }

        /// <summary>
        /// Complex integration pattern - multiple dependency patterns combined
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task ProcessOrder(Context ctx, string orderType, int quantity = 1)
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                var inventoryService = new InventoryServiceStub();
                var priceCalculator = new PriceCalculatorStub();
                
                ctx.Substitute<IOrderValidator>("📋")
                   .Substitute<IPaymentGateway>("💳")
                   .SetAlways<IInventoryService>(inventoryService, "inventory")
                   .SetOne<IPriceCalculator>(priceCalculator, "calculator");

                var orderProcessor = new OrderProcessor();
                return orderProcessor.ProcessOrder(orderType, quantity);
            }, ctx);
        }

        /// <summary>
        /// Fluent chaining with registration - demonstrates ctx.Register() method
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task RegisterUser(Context ctx, string userName, bool isAdmin = false)
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                var userService = new UserServiceStub();
                var logger = new FakeLogger();
                var authParrot = ctx.Parrot<IAuthService>("🔐");

                ctx.SetAlways<IUserService>(userService, "userSvc")
                   .SetOne<ILogger>(logger, "mainLogger")
                   .Register(authParrot, "authService");

                var coordinator = new UserCoordinator();
                return coordinator.RegisterUser(userName, isAdmin);
            }, ctx);
        }

        /// <summary>
        /// Test with minimal parameters - just Context, no additional params
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task SimpleOperation(Context ctx)
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                ctx.Substitute<IEmailService>("📧");
                
                var service = Create<IEmailService>();
                return service.SendWelcomeEmail("test@example.com", "Welcome!");
            }, ctx);
        }

        /// <summary>
        /// Exception handling test - non-Parrot exceptions should be logged and swallowed
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task HandleException(Context ctx, string input = "invalid")
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                ctx.Substitute<IValidator>("✅");
                
                // This should throw an exception that gets logged but not re-thrown
                throw new InvalidOperationException($"Test exception with input: {input}");
            }, ctx);
        }

        /// <summary>
        /// Missing return value test - ParrotMissingReturnValueException should be re-thrown
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task MissingReturnValue(Context ctx, int count = 5)
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                var validator = ctx.Parrot<IValidator>("✅");
                
                // This should call the parrot which will throw ParrotMissingReturnValueException
                validator.Validate("test input", true);
                
                return $"Validated {count} items";
            }, ctx);
        }

        /// <summary>
        /// Test void return (Task without result)
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task VoidOperation(Context ctx, string message = "Hello")
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                ctx.Substitute<ILogger>("📝");
                
                var logger = Create<ILogger>();
                logger.Log(message);
                
                // No return value - should complete without "Returns:" line
            }, ctx);
        }

        /// <summary>
        /// Context display name test - verify ctx.ToString() shows test case name
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task ContextDisplayName(Context ctx, string testScenario = "default")
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                ctx.CallLog.AppendLine($"Test scenario: {testScenario}");
                ctx.CallLog.AppendLine($"Context display name: {ctx}");
                return $"Scenario: {testScenario}, Context: {ctx}";
            }, ctx);
        }

        /// <summary>
        /// Test with complex object parameters and array return
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task ProcessOrderBatch(Context ctx, string[] orderIds, bool urgent = false)
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                ctx.Substitute<IBatchProcessor>("🔄")
                   .Substitute<INotificationService>("🔔");

                var processor = Create<IBatchProcessor>();
                return processor.ProcessBatch(orderIds, urgent);
            }, ctx);
        }

        /// <summary>
        /// Test that returns null to verify null handling
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task FindOptionalData(Context ctx, string searchTerm = "missing")
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                ctx.Substitute<IDataService>("🔍");
                
                var service = Create<IDataService>();
                return service.FindData(searchTerm);
            }, ctx);
        }

        /// <summary>
        /// Test with DateTime parameters to verify date formatting
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task ScheduleTask(Context ctx, DateTime scheduleTime, bool recurring = false)
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                ctx.Substitute<IScheduler>("⏰");
                
                var scheduler = Create<IScheduler>();
                return scheduler.ScheduleTask("Important Task", scheduleTime, recurring);
            }, ctx);
        }

        /// <summary>
        /// Multi-scenario test - demonstrates that multiple verified files are discovered and processed
        /// This test should generate multiple test cases, one for each verified file found
        /// </summary>
        [Theory]
        [SpecRecLogs]
        public async Task ProcessMultipleScenarios(Context ctx, string scenario, int count = 1)
        {
            await SpecRecExecutor.ExecuteTestAsync(async () =>
            {
                ctx.Substitute<IScenarioProcessor>("📊")
                   .Substitute<IMetricsCollector>("📈");

                var processor = Create<IScenarioProcessor>();
                var metrics = Create<IMetricsCollector>();
                
                metrics.RecordScenarioStart(scenario);
                var result = processor.ProcessScenario(scenario, count);
                metrics.RecordScenarioEnd(scenario, result);
                
                return $"Scenario '{scenario}' processed {count} times with result: {result}";
            }, ctx);
        }
    }

    // Additional test interfaces for the integration tests
    public interface IAuditService
    {
        void LogTransaction(decimal amount, string currency);
        string GetTransactionId();
    }

    public interface IEmailService
    {
        string SendWelcomeEmail(string recipient, string message);
        bool ValidateEmailAddress(string email);
    }

    public interface IBatchProcessor
    {
        string ProcessBatch(string[] orderIds, bool urgent);
        int GetProcessedCount();
    }

    public interface IDataService
    {
        string? FindData(string searchTerm);
        bool DataExists(string searchTerm);
    }

    public interface IScheduler
    {
        string ScheduleTask(string taskName, DateTime scheduleTime, bool recurring);
        void CancelTask(string taskId);
    }

    public interface INotificationService
    {
        void SendUrgentNotification(string message);
        void SendStandardNotification(string message);
    }

    public interface IScenarioProcessor
    {
        string ProcessScenario(string scenario, int count);
        bool ValidateScenario(string scenario);
    }

    public interface IMetricsCollector
    {
        void RecordScenarioStart(string scenario);
        void RecordScenarioEnd(string scenario, string result);
        int GetProcessedCount();
    }

    // Enhanced PaymentService that uses audit service for SpecRec tests
    public class EnhancedPaymentService
    {
        public string ProcessPayment(decimal amount, string currency)
        {
            var processor = Create<IPaymentProcessor>();
            var logger = Create<ILogger>();
            var auditor = Create<IAuditService>();
            
            logger.Log($"Processing payment of {amount} {currency}");
            auditor.LogTransaction(amount, currency);
            
            var result = processor.ProcessPayment(amount, currency);
            var transactionId = auditor.GetTransactionId();
            
            logger.Log($"Payment result: {result}, Transaction: {transactionId}");
            
            return $"Payment {result} - Transaction: {transactionId}";
        }
    }
}