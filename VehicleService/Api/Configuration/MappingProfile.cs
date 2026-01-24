using AutoMapper;
using VehicleService.Api.Contracts;
using VehicleService.Application.Features.CreateVehicle;

namespace VehicleService.Api.Configuration
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateVehicleRequest, CreateVehicleCommand>();
        }
    }
}
