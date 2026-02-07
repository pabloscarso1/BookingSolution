using FluentValidation;
using Common.Application.Common;
using BookingService.Application.Dtos;
using BookingService.Application.Interfaces;

namespace BookingService.Application.Features.Commands
{
    public record CreateBookingCommand(
        Guid UserId,
        Guid VehicleId,
        DateTime StartDate,
        DateTime EndDate,
        decimal BookingCost);

    public class CreateBookingHandler
    {
        private readonly IBookingRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateBookingCommand> _validator;

        public CreateBookingHandler(
            IBookingRepository repository,
            IUnitOfWork uof,
            IValidator<CreateBookingCommand> validator)
        {
            _repository = repository;
            _unitOfWork = uof;
            _validator = validator;
        }

        public async Task<Result<BookingDto>> Handle(CreateBookingCommand command)
        {
            return await _validator.ValidateAndExecuteAsync(command, async () =>
            {
                var existing = await _repository.GetAsync(x =>
                    x.UserId == command.UserId &&
                    x.VehicleId == command.VehicleId &&
                    x.StartDate == command.StartDate &&
                    x.EndDate == command.EndDate);

                if (existing is not null)
                    return Result<BookingDto>.Failure("BOOKING_ALREADY_EXISTS");

                var booking = new Domain.Booking(
                    Guid.NewGuid(),
                    command.UserId,
                    command.VehicleId,
                    command.StartDate,
                    command.EndDate,
                    DateTime.UtcNow,
                    command.BookingCost);

                await _repository.AddAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                var bookingDto = new BookingDto(
                    booking.Id,
                    booking.UserId,
                    booking.VehicleId,
                    booking.BookingCost,
                    booking.StartDate,
                    booking.EndDate,
                    booking.CreatedAt);

                return Result<BookingDto>.Success(bookingDto);
            });
        }
    }

    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("El usuario es requerido");

            RuleFor(x => x.VehicleId)
                .NotEmpty()
                .WithMessage("El vehículo es requerido");

            RuleFor(x => x.StartDate)
                .NotEmpty()
                .WithMessage("La fecha de inicio es requerida");

            RuleFor(x => x.EndDate)
                .NotEmpty()
                .WithMessage("La fecha de fin es requerida")
                .GreaterThan(x => x.StartDate)
                .WithMessage("La fecha de fin debe ser mayor a la fecha de inicio");

            RuleFor(x => x.BookingCost)
                .GreaterThan(0)
                .WithMessage("El costo de la reserva debe ser mayor a 0");
        }
    }
}
