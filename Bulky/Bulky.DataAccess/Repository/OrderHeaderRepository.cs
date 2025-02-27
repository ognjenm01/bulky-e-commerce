using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public void Update(OrderHeader orderHeader)
        {
            _context.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            OrderHeader header = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (header != null)
            {
                header.OrderStatus = orderStatus;
                if(!string.IsNullOrEmpty(paymentStatus))
                    header.PaymentStatus = paymentStatus;
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            OrderHeader header = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (header != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                    header.SessionId = sessionId;
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    header.PaymentIntentId = paymentIntentId;
                    header.PaymentDate = DateTime.Now;
                }
            }
        }
    }
}
