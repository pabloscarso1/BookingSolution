using AutoMapper;
using AuthService.Api.Contracts;
using AuthService.Application.Features.CreateVehicle;

namespace AuthService.Api.Configuration
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateVehicleRequest, CreateVehicleCommand>();
        }
    }
}
