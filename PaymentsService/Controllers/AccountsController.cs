using Microsoft.AspNetCore.Mvc;
using PaymentsService.Models.DTOs;
using PaymentsService.UseCases.CreateAccount;
using PaymentsService.UseCases.DepositFunds;
using PaymentsService.UseCases.GetBalance;

namespace PaymentsService.Controllers
{
    /// <summary>
    /// Контроллер для управления счетами пользователей.
    /// Позволяет создавать аккаунты, проверять баланс и вносить средства.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController(
        ICreateAccountService createAccountService,
        IDepositFundsService depositFundsService,
        IGetBalanceService getBalanceService) : ControllerBase
    {
        private readonly ICreateAccountService _createAccountService = createAccountService;
        private readonly IDepositFundsService _depositFundsService = depositFundsService;
        private readonly IGetBalanceService _getBalanceService = getBalanceService;

        /// <summary>
        /// Создает новый финансовый счет для пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromQuery] Guid userId)
        {
            Guid accountId = await _createAccountService.CreateAccountAsync(userId, HttpContext.RequestAborted);
            return Ok(new { AccountId = accountId, UserId = userId });
        }

        /// <summary>
        /// Пополняет баланс счета пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <param name="request">Данные платежа (сумма).</param>
        [HttpPost("{userId}/deposit")]
        public async Task<IActionResult> Deposit(
            [FromRoute] Guid userId,
            [FromBody] DepositRequest request)
        {
            decimal newBalance = await _depositFundsService.DepositAsync(userId, request.Amount, HttpContext.RequestAborted);
            return Ok(new { NewBalance = newBalance });
        }

        [HttpGet("{userId}/balance")]
        public async Task<IActionResult> GetBalance([FromRoute] Guid userId)
        {
            decimal balance = await _getBalanceService.GetBalanceAsync(userId, HttpContext.RequestAborted);
            return Ok(new { Balance = balance });
        }
    }

}
