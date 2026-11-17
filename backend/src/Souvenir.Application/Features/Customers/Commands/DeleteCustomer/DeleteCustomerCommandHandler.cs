using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Customers;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Customers.Commands.DeleteCustomer;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, CustomerResult<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerResult<bool>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.KhachHangs
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            return CustomerResult<bool>.Failed($"Không tìm thấy khách hàng với mã #{request.Id}.", 404);
        }

        _context.KhachHangs.Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return CustomerResult<bool>.Success(true, "Xóa hồ sơ khách hàng thành công.");
    }
}
