using GeekShopping.Email.Messages;
using GeekShopping.Email.Model;
using GeekShopping.Email.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.Email.Repository
{
    public class EmailRepository : IEmailRepository
    {
        private readonly DbContextOptions<MySqlContext> _mySqlContext;

        public EmailRepository(DbContextOptions<MySqlContext> mySqlContext)
        {
            _mySqlContext = mySqlContext;
        }

        public async Task LogEmail(UpdatePaymentResultMessage updatePaymentResultMessage)
        {
            EmailLog emailLog = new()
            {
                Email = updatePaymentResultMessage.Email,
                SentDate = DateTime.Now,
                Log = $"Order - {updatePaymentResultMessage.OrderId} has been created successfully"
            };

            await using var _db = new MySqlContext(_mySqlContext);
            _db.Emails.Add(emailLog);
            await _db.SaveChangesAsync();
        }
    }
}
