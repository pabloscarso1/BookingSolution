using AutoMapper;
using BookingService.Api.Contracts;
using BookingService.Application.Features.Commands;

namespace BookingService.Api.Configuration
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateBookingRequest, CreateBookingCommand>();
        }
    }
}
