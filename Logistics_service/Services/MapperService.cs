using AutoMapper;
using Logistics_service.Models;
using Logistics_service.Models.Orders;
using Logistics_service.Models.Users;
using Logistics_service.ViewModels;
using Logistics_service.ViewModels.OrderModels;

namespace Logistics_service.Services
{
    public class MapperService : Profile
    {
        public MapperService()
        {
            CreateMap<UserInputModel, User>();
            CreateMap<User, UserOutputModel>();

            CreateMap<CustomerOrderInputModel, CustomerOrder>()
                .AfterMap((src, dest) => dest.CreatedAt = DateTime.Now);
            CreateMap<CustomerOrder, CustomerOrderOutputModel>();

            CreateMap<Point, PointOutputModel>();

            CreateMap<Models.Route, RouteOutputModel>()
                .ForMember(dest => dest.RoutePoints, opt => opt.MapFrom((src, dest, destMember, context) =>
                {
                    var mapper = context.Mapper;
                    return src.RoutePoints
                        .OrderBy(rp => rp.OrderIndex)
                        .Select(rp => mapper.Map<PointOutputModel>(rp.Point))
                        .ToArray();
                }))
                .ForMember(dest => dest.Points, opt => opt.MapFrom(src => src.Points));

            CreateMap<VehicleInputModel, Vehicle>();
            CreateMap<Vehicle, VehicleOutputModel>()
                .ForMember(dest => dest.Garage, opt => opt.MapFrom(src => src.Garage))
                .ForMember(dest => dest.CurrentRoute, opt => opt.MapFrom(src => src.CurrentRoute))
                .ForMember(dest => dest.Routes, opt => opt.MapFrom((src, dest, destMember, context) =>
                {
                    var mapper = context.Mapper;
                    return new SortedDictionary<DateTime, RouteOutputModel>(
                        src.Routes.ToDictionary(
                            kvp => kvp.Key,
                            kvp => mapper.Map<RouteOutputModel>(kvp.Value)
                        ));
                }));

            CreateMap<ReadyOrder, ReadyOrderOutputModel>()
                .ForMember(dest => dest.Route, opt => opt.MapFrom(src => src.Route))
                .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle));
        }
    }
}
